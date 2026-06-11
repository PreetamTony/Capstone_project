using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Admin;
using HospitalManagement.BusinessLogic.DTOs.Visit;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Interfaces;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public class ConsultationService : IConsultationService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBillingService _billingService;
    private readonly INotificationService _notificationService;

    public ConsultationService(IUnitOfWork uow, ICurrentUserService currentUserService, IBillingService billingService, INotificationService notificationService)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _billingService = billingService;
        _notificationService = notificationService;
    }

    public async Task<ConsultationDetailsDto> CreateConsultationAsync(CreateConsultationRequestDto request, CancellationToken ct = default)
    {
        var visit = await _uow.Visits.Query()
            .Include(v => v.Doctor)
            .FirstOrDefaultAsync(v => v.Id == request.VisitId, ct)
            ?? throw new NotFoundException("Visit", request.VisitId);

        // Validation: Ensure doctor owns the visit
        if (visit.Doctor.UserId != _currentUserService.UserId)
            throw new HospitalManagement.DataAccess.Exceptions.UnauthorizedAccessException("You can only create consultations for your assigned visits.");

        // Validation: Visit must be active
        if (visit.Status != VisitStatus.CheckedIn && visit.Status != VisitStatus.InConsultation)
            throw new BusinessRuleViolationException("InvalidVisitState", "Consultation can only be created for CheckedIn or InConsultation visits.");

        // Check if consultation already exists
        var existing = await _uow.Consultations.Query().AnyAsync(c => c.VisitId == visit.Id, ct);
        if (existing)
            throw new BusinessRuleViolationException("DuplicateConsultation", "A consultation already exists for this visit.");

        var consultation = new Consultation
        {
            VisitId = visit.Id,
            DoctorId = visit.DoctorId,
            ChiefComplaint = request.ChiefComplaint,
            Symptoms = request.Symptoms ?? new List<string>(),
            Assessment = request.Assessment,
            Diagnosis = request.Diagnosis,
            DiagnosisCode = request.DiagnosisCode,
            TreatmentPlan = request.TreatmentPlan,
            Notes = request.Notes,
            Recommendations = request.Recommendations,
            FollowUpInstructions = request.FollowUpInstructions,
            FollowUpDate = request.FollowUpDate,
            Status = ConsultationStatus.InProgress,
            StartedAt = DateTime.UtcNow
        };

        // Transition visit status
        if (visit.Status == VisitStatus.CheckedIn)
        {
            visit.Status = VisitStatus.InConsultation;
            _uow.Visits.Update(visit);
        }

        await _uow.Consultations.AddAsync(consultation, ct);
        _uow.Visits.Update(visit);

        await _uow.CompleteAsync(ct);

        await LogAuditAsync("CONSULTATION_STARTED", consultation.Id.ToString(), null, new { consultation.Status });

        await _notificationService.NotifyConsultationStartedAsync(visit.PatientId, consultation.Id, ct);

        return await GetConsultationByIdAsync(consultation.Id, ct);
    }

    public async Task<ConsultationDetailsDto> GetConsultationByIdAsync(Guid id, CancellationToken ct = default)
    {
        var consultation = await _uow.Consultations.Query()
            .Include(c => c.Doctor)
            .Include(c => c.Prescriptions)
            .Include(c => c.LabReports)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Consultation", id);

        return MapToDetailsDto(consultation);
    }

    public async Task<ConsultationDetailsDto> GetConsultationByVisitIdAsync(Guid visitId, CancellationToken ct = default)
    {
        var consultation = await _uow.Consultations.Query()
            .Include(c => c.Doctor)
            .Include(c => c.Prescriptions)
            .Include(c => c.LabReports)
            .FirstOrDefaultAsync(c => c.VisitId == visitId, ct)
            ?? throw new NotFoundException("Consultation for Visit", visitId);

        return MapToDetailsDto(consultation);
    }

    public async Task<ConsultationDetailsDto> UpdateConsultationAsync(Guid id, UpdateConsultationRequestDto request, CancellationToken ct = default)
    {
        var consultation = await _uow.Consultations.Query()
            .Include(c => c.Doctor)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Consultation", id);

        EnsureDoctorOwnership(consultation);
        EnsureEditable(consultation);

        var oldValues = new { consultation.Assessment, consultation.Diagnosis, consultation.TreatmentPlan };

        consultation.ChiefComplaint = request.ChiefComplaint;
        consultation.Symptoms = request.Symptoms ?? new List<string>();
        consultation.Assessment = request.Assessment;
        consultation.Diagnosis = request.Diagnosis;
        consultation.DiagnosisCode = request.DiagnosisCode;
        consultation.TreatmentPlan = request.TreatmentPlan;
        consultation.Notes = request.Notes;
        consultation.Recommendations = request.Recommendations;
        consultation.FollowUpInstructions = request.FollowUpInstructions;
        consultation.FollowUpDate = request.FollowUpDate;

        _uow.Consultations.Update(consultation);
        await _uow.CompleteAsync(ct);

        await LogAuditAsync("CONSULTATION_UPDATED", consultation.Id.ToString(), oldValues, new { consultation.Assessment, consultation.Diagnosis, consultation.TreatmentPlan });

        return await GetConsultationByIdAsync(id, ct);
    }

    public async Task<ConsultationDetailsDto> CompleteConsultationAsync(Guid id, CancellationToken ct = default)
    {
        var consultation = await _uow.Consultations.Query()
            .Include(c => c.Doctor)
            .Include(c => c.Visit)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Consultation", id);

        EnsureDoctorOwnership(consultation);
        ConsultationStateMachine.ValidateTransition(consultation.Status, ConsultationStatus.Completed);

        if (string.IsNullOrWhiteSpace(consultation.Diagnosis) || string.IsNullOrWhiteSpace(consultation.TreatmentPlan))
            throw new BusinessRuleViolationException("IncompleteConsultation", "Diagnosis and Treatment Plan are required to complete the consultation.");

        consultation.Status = ConsultationStatus.Completed;
        consultation.CompletedAt = DateTime.UtcNow;

        _uow.Consultations.Update(consultation);
        _uow.Visits.Update(consultation.Visit);

        await _uow.CompleteAsync(ct);

        await LogAuditAsync("CONSULTATION_COMPLETED", consultation.Id.ToString(), new { Status = "Active" }, new { Status = "Completed" });

        await _notificationService.NotifyConsultationCompletedAsync(consultation.Visit.PatientId, consultation.Id, ct);

        // TRIGGER BILLING AUTOMATICALLY
        await _billingService.GenerateInvoiceForVisitAsync(consultation.VisitId, ct);

        return await GetConsultationByIdAsync(id, ct);
    }

    public async Task<ConsultationDetailsDto> CancelConsultationAsync(Guid id, string reason, CancellationToken ct = default)
    {
        var consultation = await _uow.Consultations.Query()
            .Include(c => c.Doctor)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Consultation", id);

        EnsureDoctorOwnership(consultation);
        ConsultationStateMachine.ValidateTransition(consultation.Status, ConsultationStatus.Cancelled);

        var oldStatus = consultation.Status;
        consultation.Status = ConsultationStatus.Cancelled;

        _uow.Consultations.Update(consultation);
        await _uow.CompleteAsync(ct);

        await LogAuditAsync("CONSULTATION_CANCELLED", consultation.Id.ToString(), new { Status = oldStatus }, new { Status = consultation.Status, Reason = reason });

        return await GetConsultationByIdAsync(id, ct);
    }

    public async Task<PagedResult<ConsultationSummaryDto>> GetConsultationsAsync(ConsultationFilterDto filter, CancellationToken ct = default)
    {
        var query = _uow.Consultations.Query()
            .Include(c => c.Doctor)
            .AsNoTracking()
            .AsQueryable();

        if (filter.DoctorId.HasValue)
            query = query.Where(c => c.DoctorId == filter.DoctorId.Value);
        if (filter.VisitId.HasValue)
            query = query.Where(c => c.VisitId == filter.VisitId.Value);
        if (filter.PatientId.HasValue)
            query = query.Where(c => c.Visit != null && c.Visit.PatientId == filter.PatientId.Value);
        if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<ConsultationStatus>(filter.Status, true, out var statusEnum))
            query = query.Where(c => c.Status == statusEnum);
        if (filter.FromDate.HasValue)
            query = query.Where(c => c.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(c => c.CreatedAt <= filter.ToDate.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var dtos = items.Select(c => new ConsultationSummaryDto
        {
            Id = c.Id,
            VisitId = c.VisitId,
            DoctorName = c.Doctor != null ? $"Dr. {c.Doctor.FirstName} {c.Doctor.LastName}" : "Unknown",
            ChiefComplaint = c.ChiefComplaint,
            Diagnosis = c.Diagnosis,
            Status = c.Status.ToString(),
            CreatedAt = c.CreatedAt,
            CompletedAt = c.CompletedAt
        }).ToList();

        return PagedResult<ConsultationSummaryDto>.Create(dtos, total, filter.PageNumber, filter.PageSize);
    }

    // --- Private Helpers ---

    private void EnsureDoctorOwnership(Consultation consultation)
    {
        if (consultation.Doctor != null && consultation.Doctor.UserId != _currentUserService.UserId)
            throw new HospitalManagement.DataAccess.Exceptions.UnauthorizedAccessException("You can only modify your own consultations.");
    }

    private void EnsureEditable(Consultation consultation)
    {
        if (consultation.Status == ConsultationStatus.Completed || consultation.Status == ConsultationStatus.Cancelled)
            throw new BusinessRuleViolationException("ReadOnlyConsultation", $"Consultation cannot be modified because it is {consultation.Status}.");
    }

    private async Task LogAuditAsync(string action, string recordId, object oldValues, object newValues)
    {
        // For now, bypass writing to AuditLogs to avoid missing DbSet errors.
        // It can be implemented when AuditLog infrastructure is fully wired.
        await Task.CompletedTask;
    }

    private static ConsultationDetailsDto MapToDetailsDto(Consultation c) => new()
    {
        Id = c.Id,
        VisitId = c.VisitId,
        Doctor = c.Doctor != null ? new ConsultationDoctorDto
        {
            Id = c.Doctor.UserId,
            FullName = $"Dr. {c.Doctor.FirstName} {c.Doctor.LastName}",
            Role = "Doctor"
        } : null,
        ChiefComplaint = c.ChiefComplaint,
        Symptoms = c.Symptoms ?? new List<string>(),
        Assessment = c.Assessment,
        Diagnosis = c.Diagnosis,
        DiagnosisCode = c.DiagnosisCode,
        TreatmentPlan = c.TreatmentPlan,
        Notes = c.Notes,
        Recommendations = c.Recommendations,
        FollowUpInstructions = c.FollowUpInstructions,
        FollowUpDate = c.FollowUpDate,
        Status = c.Status.ToString(),
        StartedAt = c.StartedAt,
        CompletedAt = c.CompletedAt,
        CreatedAt = c.CreatedAt,
        PrescriptionCount = c.Prescriptions?.Count ?? 0,
        LabOrderCount = c.LabReports?.Count ?? 0
    };
}
