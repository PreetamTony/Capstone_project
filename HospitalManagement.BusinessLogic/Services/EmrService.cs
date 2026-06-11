using HospitalManagement.BusinessLogic.DTOs.Emr;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models.Emr;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public partial class EmrService : IEmrService
{
    private readonly IUnitOfWork _unitOfWork;

    public EmrService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EmrRecordDto> GetEmrByPatientIdAsync(Guid patientId, CancellationToken ct = default)
    {
        var emr = await _unitOfWork.EmrRecords.GetByPatientIdWithDetailsAsync(patientId, ct);
        if (emr == null)
        {
            throw new NotFoundException($"EMR for patient {patientId} not found.");
        }

        return MapToDto(emr);
    }

    public async Task<FullEmrResponseDto> GetFullEmrAsync(Guid patientId, CancellationToken ct = default)
    {
        var emr = await _unitOfWork.EmrRecords.Query()
            .Include(e => e.Patient)
            .Include(e => e.Allergies)
            .Include(e => e.MedicalHistories)
            .FirstOrDefaultAsync(e => e.PatientId == patientId, ct)
            ?? throw new NotFoundException($"EMR for patient {patientId} not found.");

        var visits = await _unitOfWork.Visits.Query()
            .Include(v => v.Doctor)
            .Include(v => v.Vitals)
            .Include(v => v.Consultation!)
                .ThenInclude(c => c.Prescriptions)
            .Include(v => v.Consultation!)
                .ThenInclude(c => c.LabReports)
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.CheckInTime)
            .ToListAsync(ct);

        return new FullEmrResponseDto
        {
            EmrRecordId = emr.Id,
            PatientId = emr.PatientId,
            PatientName = emr.Patient != null ? $"{emr.Patient.FirstName} {emr.Patient.LastName}" : string.Empty,
            CreatedAt = emr.CreatedAt,
            Allergies = emr.Allergies.Select(a => new AllergyDto
            {
                Id = a.Id, Substance = a.Substance, Severity = a.Severity, Reaction = a.Reaction, Notes = a.Notes
            }).ToList(),
            MedicalHistory = emr.MedicalHistories.Select(m => new MedicalHistoryDto
            {
                Id = m.Id, Condition = m.Condition, DiagnosisDate = m.DiagnosisDate, Status = m.Status, Notes = m.Notes
            }).ToList(),
            PastVisits = visits.Select(v => new VisitSummaryDto
            {
                VisitId = v.Id,
                CheckInTime = v.CheckInTime,
                DischargeTime = v.DischargeTime,
                DoctorName = v.Doctor != null ? $"Dr. {v.Doctor.FirstName} {v.Doctor.LastName}" : string.Empty,
                ChiefComplaint = v.ChiefComplaint ?? string.Empty,
                Diagnosis = v.Consultation?.Diagnosis ?? string.Empty,
                TreatmentPlan = string.Empty,
                Notes = v.Notes ?? string.Empty,
                Vitals = v.Vitals.FirstOrDefault() != null ? new VitalsDto
                {
                    HeartRate = v.Vitals.First().HeartRate,
                    BloodPressure = v.Vitals.First().BloodPressure,
                    Temperature = v.Vitals.First().Temperature
                } : null,
                PrescribedMedications = (v.Consultation?.Prescriptions ?? new List<Prescription>()).SelectMany(p => p.Items).Select(i => $"{i.MedicationName} - {i.Dosage}").ToList(),
                LabReports = (v.Consultation?.LabReports ?? new List<LabReport>()).Select(l => $"{l.ReportName} ({l.Status})").ToList()
            }).ToList()
        };
    }

    public async Task<EmrRecordDto> InitializeEmrAsync(Guid patientId, InitializeEmrRequestDto request, CancellationToken ct = default)
    {
        var patientExists = await _unitOfWork.Patients.AnyAsync(p => p.Id == patientId, ct);
        if (!patientExists)
            throw new NotFoundException($"Patient {patientId} not found.");

        var existingEmr = await _unitOfWork.EmrRecords.FirstOrDefaultAsync(e => e.PatientId == patientId, ct);
        if (existingEmr != null)
            throw new BusinessRuleViolationException("EMRExists", "EMR already exists for this patient.");

        var newEmr = new EmrRecord
        {
            PatientId = patientId,
            FamilyHistory = request.FamilyHistory,
            SocialHistory = request.SocialHistory
        };

        await _unitOfWork.EmrRecords.AddAsync(newEmr, ct);
        await _unitOfWork.CompleteAsync(ct);

        return MapToDto(newEmr);
    }

    public async Task<AllergyDto> AddAllergyAsync(Guid patientId, CreateAllergyRequestDto request, CancellationToken ct = default)
    {
        var emr = await GetOrThrowEmrAsync(patientId, ct);

        var allergy = new Allergy
        {
            EmrRecordId = emr.Id,
            Substance = request.Substance,
            Severity = request.Severity,
            Reaction = request.Reaction,
            Notes = request.Notes
        };

        emr.Allergies.Add(allergy);
        await _unitOfWork.CompleteAsync(ct);

        return new AllergyDto
        {
            Id = allergy.Id,
            Substance = allergy.Substance,
            Severity = allergy.Severity,
            Reaction = allergy.Reaction,
            Notes = allergy.Notes,
            CreatedAt = allergy.CreatedAt
        };
    }

    public async Task<MedicalHistoryDto> AddMedicalHistoryAsync(Guid patientId, CreateMedicalHistoryRequestDto request, CancellationToken ct = default)
    {
        var emr = await GetOrThrowEmrAsync(patientId, ct);

        var history = new MedicalHistory
        {
            EmrRecordId = emr.Id,
            Condition = request.Condition,
            DiagnosisDate = request.DiagnosisDate,
            Status = request.Status,
            Notes = request.Notes
        };

        emr.MedicalHistories.Add(history);
        await _unitOfWork.CompleteAsync(ct);

        return new MedicalHistoryDto
        {
            Id = history.Id,
            Condition = history.Condition,
            DiagnosisDate = history.DiagnosisDate,
            Status = history.Status,
            Notes = history.Notes,
            CreatedAt = history.CreatedAt
        };
    }

    public async Task<VitalsDto> AddVitalsAsync(Guid patientId, CreateVitalsRequestDto request, CancellationToken ct = default)
    {
        var emr = await GetOrThrowEmrAsync(patientId, ct);
        
        if (request.VisitId.HasValue)
        {
            var visitExists = await _unitOfWork.Visits.AnyAsync(v => v.Id == request.VisitId.Value && v.PatientId == patientId, ct);
            if (!visitExists)
                throw new BusinessRuleViolationException("InvalidVisit", "Visit not found or does not belong to the patient.");
        }

        var vitals = new Vitals
        {
            EmrRecordId = emr.Id,
            VisitId = request.VisitId,
            RecordedAt = request.RecordedAt ?? DateTime.UtcNow,
            HeartRate = request.HeartRate,
            BloodPressure = request.BloodPressure,
            Temperature = request.Temperature,
            RespiratoryRate = request.RespiratoryRate,
            O2Saturation = request.O2Saturation,
            Height = request.Height,
            Weight = request.Weight
        };

        emr.Vitals.Add(vitals);
        await _unitOfWork.CompleteAsync(ct);

        return new VitalsDto
        {
            Id = vitals.Id,
            VisitId = vitals.VisitId,
            RecordedAt = vitals.RecordedAt,
            HeartRate = vitals.HeartRate,
            BloodPressure = vitals.BloodPressure,
            Temperature = vitals.Temperature,
            RespiratoryRate = vitals.RespiratoryRate,
            O2Saturation = vitals.O2Saturation,
            Height = vitals.Height,
            Weight = vitals.Weight,
            CreatedAt = vitals.CreatedAt
        };
    }

    private async Task<EmrRecord> GetOrThrowEmrAsync(Guid patientId, CancellationToken ct)
    {
        var emr = await _unitOfWork.EmrRecords.GetByPatientIdWithDetailsAsync(patientId, ct);
        if (emr == null)
            throw new NotFoundException($"EMR for patient {patientId} not found.");
        return emr;
    }

    private static EmrRecordDto MapToDto(EmrRecord emr)
    {
        return new EmrRecordDto
        {
            Id = emr.Id,
            PatientId = emr.PatientId,
            FamilyHistory = emr.FamilyHistory,
            SocialHistory = emr.SocialHistory,
            CreatedAt = emr.CreatedAt,
            UpdatedAt = emr.UpdatedAt,
            Allergies = emr.Allergies.Select(a => new AllergyDto
            {
                Id = a.Id,
                Substance = a.Substance,
                Severity = a.Severity,
                Reaction = a.Reaction,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt
            }).ToList(),
            MedicalHistories = emr.MedicalHistories.Select(m => new MedicalHistoryDto
            {
                Id = m.Id,
                Condition = m.Condition,
                DiagnosisDate = m.DiagnosisDate,
                Status = m.Status,
                Notes = m.Notes,
                CreatedAt = m.CreatedAt
            }).ToList(),
            Vitals = emr.Vitals.Select(v => new VitalsDto
            {
                Id = v.Id,
                VisitId = v.VisitId,
                RecordedAt = v.RecordedAt,
                HeartRate = v.HeartRate,
                BloodPressure = v.BloodPressure,
                Temperature = v.Temperature,
                RespiratoryRate = v.RespiratoryRate,
                O2Saturation = v.O2Saturation,
                Height = v.Height,
                Weight = v.Weight,
                CreatedAt = v.CreatedAt
            }).ToList()
        };
    }
}
