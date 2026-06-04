using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Prescription;
using HospitalManagement.DataAccess.Constants;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Repositories;
using HospitalManagement.DataAccess.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.BusinessLogic.Services;

/// <summary>
/// Manages prescriptions with enforced 30-minute edit window and no-delete (void only) policy.
/// </summary>
public class PrescriptionService : IPrescriptionService
{
    private readonly IUnitOfWork _uow;

    private readonly ILogger<PrescriptionService> _logger;

    public PrescriptionService(IUnitOfWork uow, ILogger<PrescriptionService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PrescriptionResponseDto> CreatePrescriptionAsync(
        Guid doctorUserId, CreatePrescriptionRequestDto request, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId, ct)
            ?? throw new NotFoundException("Doctor profile not found.");

        var visit = await _uow.Visits.Query()
            .Include(v => v.Patient)
            .FirstOrDefaultAsync(v => v.Id == request.VisitId, ct)
            ?? throw new NotFoundException("Visit", request.VisitId);

        // Doctor must own this visit
        if (visit.DoctorId != doctor.Id)
            throw new HospitalManagement.DataAccess.Exceptions.UnauthorizedAccessException(
                "You can only prescribe for visits assigned to you.");

        if (visit.Status != VisitStatus.InProgress &&
            visit.Status != VisitStatus.Discharged)
            throw new BusinessRuleViolationException("InvalidStatus",
                "Prescriptions can only be created for InProgress or Discharged visits.");

        var now = DateTime.UtcNow;
        var prescription = new Prescription
        {
            VisitId = visit.Id,
            DoctorId = doctor.Id,
            PatientId = visit.PatientId,
            MedicationName = request.MedicationName,
            Dosage = request.Dosage,
            Frequency = request.Frequency,
            DurationDays = request.DurationDays,
            Instructions = request.Instructions,
            CreatedAt = now,
            EditableUntil = now.AddMinutes(AppConstants.Prescription.EditWindowMinutes)
        };

        await _uow.Prescriptions.AddAsync(prescription, ct);
        await _uow.CompleteAsync(ct);


        _logger.LogInformation("Prescription {Id} created by Doctor {DoctorId} for Patient {PatientId}",
            prescription.Id, doctor.Id, visit.PatientId);

        return MapToDto(prescription, doctor, visit.Patient);
    }

    /// <inheritdoc/>
    public async Task<PrescriptionResponseDto> UpdatePrescriptionAsync(
        Guid prescriptionId, Guid doctorUserId, UpdatePrescriptionRequestDto request, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId, ct)
            ?? throw new NotFoundException("Doctor profile not found.");

        var prescription = await GetWithNavigationAsync(prescriptionId, ct);

        if (prescription.DoctorId != doctor.Id)
            throw new HospitalManagement.DataAccess.Exceptions.UnauthorizedAccessException("You can only edit your own prescriptions.");

        if (!prescription.IsEditable)
            throw new BusinessRuleViolationException("EditWindowExpired",
                $"Prescriptions can only be edited within {AppConstants.Prescription.EditWindowMinutes} minutes of creation.");

        var old = new { prescription.MedicationName, prescription.Dosage, prescription.Frequency, prescription.DurationDays };

        if (request.MedicationName != null) prescription.MedicationName = request.MedicationName;
        if (request.Dosage != null) prescription.Dosage = request.Dosage;
        if (request.Frequency != null) prescription.Frequency = request.Frequency;
        if (request.DurationDays.HasValue) prescription.DurationDays = request.DurationDays.Value;
        if (request.Instructions != null) prescription.Instructions = request.Instructions;

        _uow.Prescriptions.Update(prescription);
        await _uow.CompleteAsync(ct);


        return MapToDto(prescription, prescription.Doctor, prescription.Patient);
    }

    /// <inheritdoc/>
    public async Task<PrescriptionResponseDto> VoidPrescriptionAsync(
        Guid prescriptionId, Guid doctorUserId, VoidPrescriptionRequestDto request, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId, ct)
            ?? throw new NotFoundException("Doctor profile not found.");

        var prescription = await GetWithNavigationAsync(prescriptionId, ct);

        if (prescription.DoctorId != doctor.Id)
            throw new HospitalManagement.DataAccess.Exceptions.UnauthorizedAccessException("You can only void your own prescriptions.");

        if (prescription.IsVoided)
            throw new BusinessRuleViolationException("AlreadyVoided", "This prescription is already voided.");

        prescription.IsVoided = true;
        prescription.VoidReason = request.Reason;

        _uow.Prescriptions.Update(prescription);
        await _uow.CompleteAsync(ct);


        return MapToDto(prescription, prescription.Doctor, prescription.Patient);
    }

    /// <inheritdoc/>
    public async Task<PrescriptionResponseDto> MarkDispensedAsync(
        Guid prescriptionId, Guid pharmacistUserId, CancellationToken ct = default)
    {
        var prescription = await GetWithNavigationAsync(prescriptionId, ct);

        if (prescription.IsVoided)
            throw new BusinessRuleViolationException("VoidedPrescription", "Cannot dispense a voided prescription.");

        if (prescription.IsDispensed)
            throw new BusinessRuleViolationException("AlreadyDispensed", "This prescription has already been dispensed.");

        prescription.IsDispensed = true;
        prescription.DispensedBy = pharmacistUserId;
        prescription.DispensedAt = DateTime.UtcNow;

        _uow.Prescriptions.Update(prescription);
        await _uow.CompleteAsync(ct);


        return MapToDto(prescription, prescription.Doctor, prescription.Patient);
    }

    /// <inheritdoc/>
    public async Task<PagedResult<PrescriptionResponseDto>> GetPatientPrescriptionsAsync(
        Guid patientId, PaginationFilter filter, CancellationToken ct = default)
    {
        var query = _uow.Prescriptions.Query()
            .Where(p => p.PatientId == patientId)
            .Include(p => p.Doctor).ThenInclude(d => d.User)
            .Include(p => p.Patient).ThenInclude(pt => pt.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(p => p.MedicationName.Contains(filter.SearchTerm));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return PagedResult<PrescriptionResponseDto>.Create(
            items.Select(p => MapToDto(p, p.Doctor, p.Patient)).ToList(),
            total, filter.PageNumber, filter.PageSize);
    }

    /// <inheritdoc/>
    public async Task<PrescriptionResponseDto> GetByIdAsync(Guid prescriptionId, CancellationToken ct = default)
    {
        var prescription = await GetWithNavigationAsync(prescriptionId, ct);
        return MapToDto(prescription, prescription.Doctor, prescription.Patient);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<Prescription> GetWithNavigationAsync(Guid id, CancellationToken ct)
        => await _uow.Prescriptions.Query()
               .Include(p => p.Doctor).ThenInclude(d => d.User)
               .Include(p => p.Patient).ThenInclude(pt => pt.User)
               .FirstOrDefaultAsync(p => p.Id == id, ct)
           ?? throw new NotFoundException("Prescription", id);

    private static PrescriptionResponseDto MapToDto(Prescription p, Doctor d, Patient pt)
        => new()
        {
            Id = p.Id,
            VisitId = p.VisitId,
            DoctorId = d.Id,
            DoctorName = $"Dr. {d.FirstName} {d.LastName}",
            PatientId = pt.Id,
            PatientName = $"{pt.FirstName} {pt.LastName}",
            MedicationName = p.MedicationName,
            Dosage = p.Dosage,
            Frequency = p.Frequency,
            DurationDays = p.DurationDays,
            Instructions = p.Instructions,
            IsDispensed = p.IsDispensed,
            DispensedAt = p.DispensedAt,
            IsVoided = p.IsVoided,
            VoidReason = p.VoidReason,
            IsEditable = p.IsEditable,
            EditableUntil = p.EditableUntil,
            CreatedAt = p.CreatedAt
        };
}
