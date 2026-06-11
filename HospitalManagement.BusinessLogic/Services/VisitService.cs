using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Visit;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Interfaces;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Repositories;
using HospitalManagement.BusinessLogic.DTOs.Appointment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.BusinessLogic.Services;

public class VisitService : IVisitService
{
    private readonly IUnitOfWork _uow;
    private readonly IQueueService _queueService;
    private readonly IBillingService _billingService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<VisitService> _logger;

    public VisitService(
        IUnitOfWork uow, 
        IQueueService queueService, 
        IBillingService billingService,
        ICurrentUserService currentUserService,
        ILogger<VisitService> logger)
    {
        _uow = uow;
        _queueService = queueService;
        _billingService = billingService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<VisitDetailsDto> StartVisitAsync(Guid appointmentId, StartVisitRequestDto request, CancellationToken ct = default)
    {
        var appointment = await _uow.Appointments.Query()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == appointmentId, ct)
            ?? throw new NotFoundException("Appointment", appointmentId);

        if (appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.Completed)
            throw new BusinessRuleViolationException("InvalidAppointmentStatus", "Cannot start a visit for a cancelled or completed appointment.");

        var existingVisit = await _uow.Visits.Query().AnyAsync(v => v.AppointmentId == appointmentId, ct);
        if (existingVisit)
            throw new BusinessRuleViolationException("VisitExists", "A visit already exists for this appointment.");

        var visitNumber = await GenerateVisitNumberAsync(ct);

        var visit = new Visit
        {
            VisitNumber = visitNumber,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            AppointmentId = appointment.Id,
            DepartmentId = appointment.Doctor.DepartmentId,
            ChiefComplaint = request.ChiefComplaint,
            Notes = request.Notes,
            Status = VisitStatus.CheckedIn,
            CheckInTime = DateTime.UtcNow,
            CreatedBy = _currentUserService.UserId
        };

        // Transition appointment
        appointment.Status = AppointmentStatus.InProgress;
        appointment.CheckedInAt = DateTime.UtcNow;

        // Log history
        await AddVisitHistoryAsync(visit, VisitStatus.CheckedIn, VisitStatus.CheckedIn, "Visit Started");

        await _uow.Visits.AddAsync(visit, ct);
        _uow.Appointments.Update(appointment);
        await _uow.CompleteAsync(ct);

        // Add to Queue
        await _queueService.AddToQueueAsync(appointment.PatientId, appointment.DoctorId, visit.Id, ct);

        return await GetVisitByIdAsync(visit.Id, ct);
    }

    public async Task<VisitDetailsDto> DischargeVisitAsync(Guid visitId, CancellationToken ct = default)
    {
        var visit = await _uow.Visits.Query()
            .Include(v => v.Appointment)
            .FirstOrDefaultAsync(v => v.Id == visitId, ct)
            ?? throw new NotFoundException("Visit", visitId);

        VisitStateMachine.ValidateTransition(visit.Status, VisitStatus.Completed);

        await AddVisitHistoryAsync(visit, visit.Status, VisitStatus.Completed, "Visit Discharged/Completed");

        visit.Status = VisitStatus.Completed;
        visit.DischargeTime = DateTime.UtcNow;
        visit.UpdatedBy = _currentUserService.UserId;

        if (visit.Appointment != null)
        {
            visit.Appointment.Status = AppointmentStatus.Completed;
            visit.Appointment.CompletedAt = DateTime.UtcNow;
            _uow.Appointments.Update(visit.Appointment);
        }

        _uow.Visits.Update(visit);
        await _uow.CompleteAsync(ct);

        // Trigger billing
        await _billingService.GenerateInvoiceForVisitAsync(visit.Id, ct);

        return await GetVisitByIdAsync(visit.Id, ct);
    }

    public async Task<VisitDetailsDto> CancelVisitAsync(Guid visitId, CancelVisitRequestDto request, CancellationToken ct = default)
    {
        var visit = await _uow.Visits.Query()
            .Include(v => v.Appointment)
            .FirstOrDefaultAsync(v => v.Id == visitId, ct)
            ?? throw new NotFoundException("Visit", visitId);

        VisitStateMachine.ValidateTransition(visit.Status, VisitStatus.Cancelled);

        await AddVisitHistoryAsync(visit, visit.Status, VisitStatus.Cancelled, request.Reason ?? "Cancelled");

        visit.Status = VisitStatus.Cancelled;
        visit.CancelledAt = DateTime.UtcNow;
        visit.CancelledBy = _currentUserService.UserId;
        visit.CancellationReason = request.Reason;

        if (visit.Appointment != null && visit.Appointment.Status != AppointmentStatus.Completed)
        {
            visit.Appointment.Status = AppointmentStatus.Cancelled;
            visit.Appointment.CancellationReason = request.Reason;
            visit.Appointment.CancelledBy = _currentUserService.UserId;
            _uow.Appointments.Update(visit.Appointment);
        }

        _uow.Visits.Update(visit);
        await _uow.CompleteAsync(ct);

        return await GetVisitByIdAsync(visit.Id, ct);
    }

    public async Task<VisitDetailsDto> GetVisitByIdAsync(Guid visitId, CancellationToken ct = default)
    {
        var visit = await _uow.Visits.Query()
            .Include(v => v.Patient)
            .Include(v => v.Doctor).ThenInclude(d => d.Department)
            .Include(v => v.Appointment)
            .Include(v => v.Consultation)
            .Include(v => v.Consultation!)
                .ThenInclude(c => c.Prescriptions)
            .Include(v => v.Consultation!)
                .ThenInclude(c => c.LabReports)
            .FirstOrDefaultAsync(v => v.Id == visitId, ct)
            ?? throw new NotFoundException("Visit", visitId);

        return MapToDetailsDto(visit);
    }

    public async Task<PagedResult<VisitSummaryDto>> GetVisitsAsync(VisitFilterDto filter, CancellationToken ct = default)
    {
        var query = _uow.Visits.Query()
            .Include(v => v.Patient)
            .Include(v => v.Doctor).ThenInclude(d => d.Department)
            .AsQueryable();

        if (filter.Status.HasValue) query = query.Where(v => v.Status == filter.Status);
        if (filter.VisitType.HasValue) query = query.Where(v => v.VisitType == filter.VisitType);
        if (filter.DoctorId.HasValue) query = query.Where(v => v.DoctorId == filter.DoctorId);
        if (filter.PatientId.HasValue) query = query.Where(v => v.PatientId == filter.PatientId);
        if (filter.DepartmentId.HasValue) query = query.Where(v => v.DepartmentId == filter.DepartmentId);
        if (filter.FromDate.HasValue) query = query.Where(v => v.CheckInTime >= filter.FromDate);
        if (filter.ToDate.HasValue) query = query.Where(v => v.CheckInTime <= filter.ToDate);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(v => v.VisitNumber.Contains(filter.SearchTerm) ||
                                     v.Patient.FirstName.Contains(filter.SearchTerm) ||
                                     v.Patient.LastName.Contains(filter.SearchTerm));
        }

        query = filter.Sorting.ToLower() switch
        {
            "date_asc" => query.OrderBy(v => v.CheckInTime),
            "date_desc" => query.OrderByDescending(v => v.CheckInTime),
            _ => query.OrderByDescending(v => v.CheckInTime)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var dtos = items.Select(MapToSummaryDto).ToList();
        return PagedResult<VisitSummaryDto>.Create(dtos, total, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<VisitHistoryDto>> GetVisitHistoryAsync(Guid visitId, CancellationToken ct = default)
    {
        var exists = await _uow.Visits.Query().AnyAsync(v => v.Id == visitId, ct);
        if (!exists) throw new NotFoundException("Visit", visitId);

        var history = await _uow.VisitHistories.Query()
            .Where(h => h.VisitId == visitId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync(ct);

        return history.Select(h => new VisitHistoryDto
        {
            Id = h.Id,
            PreviousState = h.PreviousState.ToString(),
            NewState = h.NewState.ToString(),
            Reason = h.Reason,
            ChangedAt = h.ChangedAt
        }).ToList();
    }

    // --- Private Helpers ---

    private async Task<string> GenerateVisitNumberAsync(CancellationToken ct)
    {
        var count = await _uow.Visits.Query().CountAsync(ct);
        return $"VIS-{DateTime.UtcNow.Year}-{(count + 1):D6}";
    }

    private async Task AddVisitHistoryAsync(Visit visit, VisitStatus prev, VisitStatus next, string reason)
    {
        var history = new VisitHistory
        {
            VisitId = visit.Id,
            PreviousState = prev,
            NewState = next,
            Reason = reason,
            ChangedBy = _currentUserService.UserId ?? Guid.Empty,
            ChangedAt = DateTime.UtcNow
        };
        await _uow.VisitHistories.AddAsync(history, CancellationToken.None);
    }

    private static VisitSummaryDto MapToSummaryDto(Visit v)
    {
        return new VisitSummaryDto
        {
            Id = v.Id,
            VisitNumber = v.VisitNumber,
            Status = v.Status.ToString(),
            VisitType = v.VisitType.ToString(),
            CheckInTime = v.CheckInTime,
            PatientName = v.Patient != null ? $"{v.Patient.FirstName} {v.Patient.LastName}" : string.Empty,
            DoctorName = v.Doctor != null ? $"Dr. {v.Doctor.FirstName} {v.Doctor.LastName}" : string.Empty,
            DepartmentName = v.Doctor?.Department?.Name
        };
    }

    private static VisitDetailsDto MapToDetailsDto(Visit v)
    {
        var dto = new VisitDetailsDto
        {
            Id = v.Id,
            VisitNumber = v.VisitNumber,
            Status = v.Status.ToString(),
            VisitType = v.VisitType.ToString(),
            CheckInTime = v.CheckInTime,
            DischargeTime = v.DischargeTime,
            ChiefComplaint = v.ChiefComplaint,
            Notes = v.Notes,
            QueueNumber = v.QueueNumber,
            RoomNumber = v.RoomNumber
        };

        if (v.Patient != null)
        {
            dto.Patient = new VisitPatientSummary
            {
                Id = v.Patient.Id,
                FullName = $"{v.Patient.FirstName} {v.Patient.LastName}",
                Age = v.Patient.DateOfBirth == default ? 0 : DateTime.UtcNow.Year - v.Patient.DateOfBirth.Year,
                BloodGroup = v.Patient.BloodGroup?.ToString()
            };
        }

        if (v.Doctor != null)
        {
            dto.Doctor = new VisitDoctorSummary
            {
                Id = v.Doctor.Id,
                FullName = $"Dr. {v.Doctor.FirstName} {v.Doctor.LastName}",
                Specialization = v.Doctor.Specialization,
                DepartmentName = v.Doctor.Department?.Name
            };
        }

        if (v.Appointment != null)
        {
            dto.Appointment = new AppointmentSummaryDto
            {
                Id = v.Appointment.Id,
                DoctorName = dto.Doctor.FullName,
                DoctorSpecialization = dto.Doctor.Specialization,
                PatientName = dto.Patient.FullName,
                AppointmentTime = v.Appointment.AppointmentTime,
                Status = v.Appointment.Status.ToString(),
                Type = v.Appointment.Type.ToString(),
                Priority = v.Appointment.Priority.ToString()
            };
        }

        if (v.Consultation != null)
        {
            dto.Consultation = new VisitConsultationSummary
            {
                Id = v.Consultation.Id,
                Status = v.Consultation.Status.ToString(),
                Diagnosis = v.Consultation.Diagnosis,
                Recommendations = v.Consultation.Recommendations
            };
        }

        if (v.Consultation?.Prescriptions != null)
        {
            dto.Prescriptions = v.Consultation.Prescriptions.Select(p => new VisitPrescriptionSummary
            {
                Id = p.Id,
                Status = p.Status.ToString(),
                ItemCount = p.Items.Count,
                Items = p.Items.Select(i => new VisitPrescriptionItemSummary
                {
                    MedicationName = i.MedicationName,
                    Dosage = i.Dosage,
                    Frequency = i.Frequency
                }).ToList()
            }).ToList();
        }

        if (v.Consultation?.LabReports != null)
        {
            dto.LabReports = v.Consultation.LabReports.Select(l => new VisitLabReportSummary
            {
                Id = l.Id,
                ReportName = l.ReportName,
                Status = l.Status.ToString()
            }).ToList();
        }

        // Billing mappings removed. Use IBillingService for details.

        return dto;
    }
}
