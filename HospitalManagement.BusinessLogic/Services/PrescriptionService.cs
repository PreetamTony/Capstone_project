using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Prescription;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Emr;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.BusinessLogic.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PrescriptionService> _logger;
    private readonly INotificationService _notificationService;

    public PrescriptionService(IUnitOfWork uow, ILogger<PrescriptionService> logger, INotificationService notificationService)
    {
        _uow = uow;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<PrescriptionResponseDto> CreatePrescriptionAsync(Guid doctorUserId, CreatePrescriptionRequestDto request, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId, ct)
            ?? throw new NotFoundException("Doctor profile not found.");

        var consultation = await _uow.Consultations.Query()
            .Include(c => c.Visit)
            .FirstOrDefaultAsync(c => c.Id == request.ConsultationId, ct)
            ?? throw new NotFoundException("Consultation", request.ConsultationId);

        if (consultation.DoctorId != doctor.Id)
            throw new HospitalManagement.DataAccess.Exceptions.UnauthorizedAccessException("Only the consultation doctor can create a prescription.");

        if (consultation.Status == ConsultationStatus.Cancelled)
            throw new BusinessRuleViolationException("InvalidStatus", "Cannot create prescriptions for cancelled consultations.");

        var prescription = new Prescription
        {
            ConsultationId = consultation.Id,
            DoctorId = doctor.Id,
            PatientId = consultation.Visit!.PatientId,
            Status = PrescriptionStatus.Draft,
            Notes = request.Notes
        };

        await _uow.Prescriptions.AddAsync(prescription, ct);
        await _uow.CompleteAsync(ct);

        return await GetByIdAsync(prescription.Id, ct);
    }

    public async Task<PrescriptionItemDto> AddMedicationItemAsync(Guid prescriptionId, Guid doctorUserId, AddMedicationItemRequestDto request, CancellationToken ct = default)
    {
        var prescription = await GetPrescriptionForEditAsync(prescriptionId, doctorUserId, ct);

        var patientEmr = await _uow.EmrRecords.GetByPatientIdWithDetailsAsync(prescription.PatientId, ct);
        var patientAllergies = patientEmr?.Allergies ?? new HashSet<Allergy>();

        if (patientAllergies.Any(a => a.Substance.Contains(request.MedicationName, StringComparison.OrdinalIgnoreCase) || 
                                      request.MedicationName.Contains(a.Substance, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessRuleViolationException("AllergyConflictDetected", $"Patient has a known allergy conflicting with {request.MedicationName}. Addition blocked for safety.");
        }

        var item = new PrescriptionItem
        {
            PrescriptionId = prescription.Id,
            MedicationName = request.MedicationName,
            Strength = request.Strength,
            Dosage = request.Dosage,
            Frequency = request.Frequency,
            DurationDays = request.DurationDays,
            Instructions = request.Instructions,
            Quantity = request.Quantity
        };

        await _uow.PrescriptionItems.AddAsync(item, ct);
        await _uow.CompleteAsync(ct);

        return MapToItemDto(item);
    }

    public async Task<PrescriptionItemDto> UpdateMedicationItemAsync(Guid itemId, Guid doctorUserId, UpdateMedicationItemRequestDto request, CancellationToken ct = default)
    {
        var item = await _uow.PrescriptionItems.Query()
            .Include(i => i.Prescription)
            .FirstOrDefaultAsync(i => i.Id == itemId, ct)
            ?? throw new NotFoundException("PrescriptionItem", itemId);

        await GetPrescriptionForEditAsync(item.PrescriptionId, doctorUserId, ct);

        item.MedicationName = request.MedicationName;
        item.Strength = request.Strength;
        item.Dosage = request.Dosage;
        item.Frequency = request.Frequency;
        item.DurationDays = request.DurationDays;
        item.Instructions = request.Instructions;
        item.Quantity = request.Quantity;

        _uow.PrescriptionItems.Update(item);
        await _uow.CompleteAsync(ct);

        return MapToItemDto(item);
    }

    public async Task DeleteMedicationItemAsync(Guid itemId, Guid doctorUserId, CancellationToken ct = default)
    {
        var item = await _uow.PrescriptionItems.Query()
            .Include(i => i.Prescription)
            .FirstOrDefaultAsync(i => i.Id == itemId, ct)
            ?? throw new NotFoundException("PrescriptionItem", itemId);

        await GetPrescriptionForEditAsync(item.PrescriptionId, doctorUserId, ct);

        _uow.PrescriptionItems.Delete(item);
        await _uow.CompleteAsync(ct);
    }

    public async Task<PrescriptionResponseDto> FinalizePrescriptionAsync(Guid prescriptionId, Guid doctorUserId, CancellationToken ct = default)
    {
        var prescription = await GetPrescriptionForEditAsync(prescriptionId, doctorUserId, ct);

        if (prescription.Items.Count == 0)
            throw new BusinessRuleViolationException("EmptyPrescription", "Prescription must contain at least one medication item before finalization.");

        prescription.Status = PrescriptionStatus.Active;
        prescription.FinalizedAt = DateTime.UtcNow;
        prescription.ExpiresAt = DateTime.UtcNow.AddDays(30);

        _uow.Prescriptions.Update(prescription);
        await _uow.CompleteAsync(ct);

        await _notificationService.NotifyPrescriptionCreatedAsync(prescription.PatientId, prescription.Id, ct);

        return await GetByIdAsync(prescription.Id, ct);
    }

    public async Task<PrescriptionResponseDto> DispensePrescriptionAsync(Guid prescriptionId, Guid pharmacistUserId, CancellationToken ct = default)
    {
        var prescription = await _uow.Prescriptions.Query()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == prescriptionId, ct)
            ?? throw new NotFoundException("Prescription", prescriptionId);

        if (prescription.Status == PrescriptionStatus.Cancelled)
            throw new BusinessRuleViolationException("InvalidStatus", "Cannot dispense a voided prescription.");
            
        if (prescription.Status == PrescriptionStatus.Expired || (prescription.ExpiresAt.HasValue && prescription.ExpiresAt.Value < DateTime.UtcNow))
            throw new BusinessRuleViolationException("Expired", "Cannot dispense an expired prescription.");

        if (prescription.Status != PrescriptionStatus.Active && prescription.Status != PrescriptionStatus.PartiallyDispensed)
            throw new BusinessRuleViolationException("InvalidStatus", "Prescription is not active for dispensation.");

        prescription.Status = PrescriptionStatus.Dispensed;
        prescription.DispensedAt = DateTime.UtcNow;

        foreach(var item in prescription.Items)
        {
            item.IsDispensed = true;
            _uow.PrescriptionItems.Update(item);
        }

        _uow.Prescriptions.Update(prescription);
        await _uow.CompleteAsync(ct);

        return await GetByIdAsync(prescription.Id, ct);
    }

    public async Task<PrescriptionResponseDto> VoidPrescriptionAsync(Guid prescriptionId, Guid doctorUserId, VoidPrescriptionRequestDto request, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId, ct)
            ?? throw new NotFoundException("Doctor profile not found.");

        var prescription = await _uow.Prescriptions.GetByIdAsync(prescriptionId, ct)
            ?? throw new NotFoundException("Prescription", prescriptionId);

        if (prescription.DoctorId != doctor.Id)
            throw new HospitalManagement.DataAccess.Exceptions.UnauthorizedAccessException("Only the issuing doctor can void this prescription.");

        if (prescription.Status == PrescriptionStatus.Dispensed || prescription.Status == PrescriptionStatus.PartiallyDispensed)
            throw new BusinessRuleViolationException("InvalidStatus", "Dispensed prescriptions cannot be voided.");

        prescription.Status = PrescriptionStatus.Cancelled;
        prescription.Notes = $"[VOIDED: {request.Reason}] " + prescription.Notes;

        _uow.Prescriptions.Update(prescription);
        await _uow.CompleteAsync(ct);

        return await GetByIdAsync(prescription.Id, ct);
    }

    public async Task<PrescriptionResponseDto> GetByIdAsync(Guid prescriptionId, CancellationToken ct = default)
    {
        var prescription = await _uow.Prescriptions.Query()
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == prescriptionId, ct)
            ?? throw new NotFoundException("Prescription", prescriptionId);

        return MapToResponseDto(prescription);
    }

    public async Task<List<PrescriptionSummaryDto>> GetByConsultationAsync(Guid consultationId, CancellationToken ct = default)
    {
        var prescriptions = await _uow.Prescriptions.Query()
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Items)
            .Where(p => p.ConsultationId == consultationId)
            .ToListAsync(ct);

        return prescriptions.Select(MapToSummaryDto).ToList();
    }

    public async Task<PagedResult<PrescriptionSummaryDto>> GetPatientPrescriptionsAsync(Guid patientUserId, PaginationFilter filter, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.Query().FirstOrDefaultAsync(p => p.UserId == patientUserId, ct)
            ?? throw new NotFoundException("Patient account not found.");

        var query = _uow.Prescriptions.Query()
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Items)
            .Where(p => p.PatientId == patient.Id)
            .OrderByDescending(p => p.CreatedAt)
            .AsQueryable();

        var total = await query.CountAsync(ct);
        var items = await query.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(ct);

        return new PagedResult<PrescriptionSummaryDto>
        {
            Items = items.Select(MapToSummaryDto).ToList(),
            TotalCount = total,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<PagedResult<PrescriptionSummaryDto>> GetDoctorPrescriptionsAsync(Guid doctorUserId, PaginationFilter filter, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.Query().FirstOrDefaultAsync(d => d.UserId == doctorUserId, ct)
            ?? throw new NotFoundException("Doctor account not found.");

        var query = _uow.Prescriptions.Query()
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Items)
            .Where(p => p.DoctorId == doctor.Id)
            .OrderByDescending(p => p.CreatedAt)
            .AsQueryable();

        var total = await query.CountAsync(ct);
        var items = await query.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(ct);

        return new PagedResult<PrescriptionSummaryDto>
        {
            Items = items.Select(MapToSummaryDto).ToList(),
            TotalCount = total,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    private async Task<Prescription> GetPrescriptionForEditAsync(Guid prescriptionId, Guid doctorUserId, CancellationToken ct)
    {
        var doctor = await _uow.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId, ct)
            ?? throw new NotFoundException("Doctor profile not found.");

        var prescription = await _uow.Prescriptions.Query()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == prescriptionId, ct)
            ?? throw new NotFoundException("Prescription", prescriptionId);

        if (prescription.DoctorId != doctor.Id)
            throw new HospitalManagement.DataAccess.Exceptions.UnauthorizedAccessException("Only the issuing doctor can modify this prescription.");

        if (prescription.Status != PrescriptionStatus.Draft)
            throw new BusinessRuleViolationException("InvalidStatus", "Prescription cannot be modified once it is finalized, dispensed, or cancelled.");

        return prescription;
    }

    private static PrescriptionResponseDto MapToResponseDto(Prescription p)
    {
        return new PrescriptionResponseDto
        {
            Id = p.Id,
            ConsultationId = p.ConsultationId,
            Patient = new BasicUserDto { Id = p.PatientId, Name = $"{p.Patient.FirstName} {p.Patient.LastName}" },
            Doctor = new BasicUserDto { Id = p.DoctorId, Name = $"Dr. {p.Doctor.FirstName} {p.Doctor.LastName}" },
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt,
            FinalizedAt = p.FinalizedAt,
            DispensedAt = p.DispensedAt,
            ExpiresAt = p.ExpiresAt,
            Notes = p.Notes,
            Items = p.Items.Select(MapToItemDto).ToList()
        };
    }

    private static PrescriptionSummaryDto MapToSummaryDto(Prescription p)
    {
        return new PrescriptionSummaryDto
        {
            Id = p.Id,
            ConsultationId = p.ConsultationId,
            Patient = new BasicUserDto { Id = p.PatientId, Name = $"{p.Patient.FirstName} {p.Patient.LastName}" },
            Doctor = new BasicUserDto { Id = p.DoctorId, Name = $"Dr. {p.Doctor.FirstName} {p.Doctor.LastName}" },
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt,
            ExpiresAt = p.ExpiresAt,
            ItemCount = p.Items.Count
        };
    }

    private static PrescriptionItemDto MapToItemDto(PrescriptionItem i)
    {
        return new PrescriptionItemDto
        {
            Id = i.Id,
            MedicationName = i.MedicationName,
            Strength = i.Strength,
            Dosage = i.Dosage,
            Frequency = i.Frequency,
            DurationDays = i.DurationDays,
            Instructions = i.Instructions,
            Quantity = i.Quantity,
            IsDispensed = i.IsDispensed
        };
    }
}
