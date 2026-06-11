using HospitalManagement.BusinessLogic.DTOs.Emr;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models.Emr;
using HospitalManagement.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using HospitalManagement.BusinessLogic.DTOs.Patient;

namespace HospitalManagement.BusinessLogic.Services;

public partial class EmrService
{
    public async Task UpdateAllergyAsync(Guid patientId, Guid allergyId, UpdateAllergyRequestDto request, CancellationToken ct = default)
    {
        var allergy = await _unitOfWork.Allergies.Query()
            .Include(a => a.EmrRecord)
            .FirstOrDefaultAsync(a => a.Id == allergyId && a.EmrRecord.PatientId == patientId, ct)
            ?? throw new NotFoundException("Allergy not found.");

        allergy.Substance = request.Substance;
        allergy.Severity = request.Severity;
        allergy.Reaction = request.Reaction;
        allergy.Status = request.Status;
        allergy.Notes = request.Notes;

        await _unitOfWork.CompleteAsync(ct);
    }

    public async Task DeleteAllergyAsync(Guid patientId, Guid allergyId, CancellationToken ct = default)
    {
        var allergy = await _unitOfWork.Allergies.Query()
            .Include(a => a.EmrRecord)
            .FirstOrDefaultAsync(a => a.Id == allergyId && a.EmrRecord.PatientId == patientId, ct)
            ?? throw new NotFoundException("Allergy not found.");

        allergy.IsDeleted = true;
        allergy.DeletedAt = DateTime.UtcNow;

        await _unitOfWork.CompleteAsync(ct);
    }

    public async Task UpdateMedicalHistoryAsync(Guid patientId, Guid historyId, UpdateMedicalHistoryRequestDto request, CancellationToken ct = default)
    {
        var history = await _unitOfWork.MedicalHistories.Query()
            .Include(h => h.EmrRecord)
            .FirstOrDefaultAsync(h => h.Id == historyId && h.EmrRecord.PatientId == patientId, ct)
            ?? throw new NotFoundException("Medical History not found.");

        history.Condition = request.Condition;
        history.DiagnosisDate = request.DiagnosisDate;
        history.Status = request.Status;
        history.Notes = request.Notes;

        await _unitOfWork.CompleteAsync(ct);
    }

    public async Task DeleteMedicalHistoryAsync(Guid patientId, Guid historyId, CancellationToken ct = default)
    {
        var history = await _unitOfWork.MedicalHistories.Query()
            .Include(h => h.EmrRecord)
            .FirstOrDefaultAsync(h => h.Id == historyId && h.EmrRecord.PatientId == patientId, ct)
            ?? throw new NotFoundException("Medical History not found.");

        history.IsDeleted = true;
        history.DeletedAt = DateTime.UtcNow;

        await _unitOfWork.CompleteAsync(ct);
    }

    public async Task<EmrSummaryDto> GetEmrSummaryAsync(Guid patientId, CancellationToken ct = default)
    {
        var patient = await _unitOfWork.Patients.Query()
            .Include(p => p.EmrRecord)
            .FirstOrDefaultAsync(p => p.Id == patientId, ct)
            ?? throw new NotFoundException("Patient not found.");

        var emr = patient.EmrRecord;
        
        int activeAllergies = emr != null ? await _unitOfWork.Allergies.Query().CountAsync(a => a.EmrRecordId == emr.Id && a.Status == "Active", ct) : 0;
        int activeConditions = emr != null ? await _unitOfWork.MedicalHistories.Query().CountAsync(m => m.EmrRecordId == emr.Id && m.Status == "Active", ct) : 0;
        
        var latestVitals = emr != null ? await _unitOfWork.Vitals.Query()
            .Where(v => v.EmrRecordId == emr.Id)
            .OrderByDescending(v => v.RecordedAt)
            .FirstOrDefaultAsync(ct) : null;

        var lastVisit = await _unitOfWork.Visits.Query()
            .Where(v => v.PatientId == patientId && v.Status == DataAccess.Models.Enums.VisitStatus.Completed)
            .OrderByDescending(v => v.CheckInTime)
            .FirstOrDefaultAsync(ct);

        var lastConsult = await _unitOfWork.Consultations.Query()
            .Where(c => c.Visit.PatientId == patientId && c.Status == DataAccess.Models.Enums.ConsultationStatus.Completed)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);

        int totalVisits = await _unitOfWork.Visits.Query().CountAsync(v => v.PatientId == patientId, ct);
        int totalConsults = await _unitOfWork.Consultations.Query().CountAsync(c => c.Visit.PatientId == patientId, ct);

        return new EmrSummaryDto
        {
            PatientId = patient.Id,
            FullName = patient.FullName,
            Age = patient.Age,
            Gender = patient.Gender.ToString(),
            BloodGroup = patient.BloodGroup?.ToString(),
            ActiveAllergiesCount = activeAllergies,
            ActiveConditionsCount = activeConditions,
            LatestVitals = latestVitals != null ? new VitalsDto
            {
                Id = latestVitals.Id,
                VisitId = latestVitals.VisitId,
                RecordedAt = latestVitals.RecordedAt,
                HeartRate = latestVitals.HeartRate,
                BloodPressure = latestVitals.BloodPressure,
                Temperature = latestVitals.Temperature,
                RespiratoryRate = latestVitals.RespiratoryRate,
                O2Saturation = latestVitals.O2Saturation,
                Height = latestVitals.Height,
                Weight = latestVitals.Weight
            } : null,
            LastVisitDate = lastVisit?.CheckInTime,
            LastConsultationDate = lastConsult?.CreatedAt,
            TotalVisits = totalVisits,
            TotalConsultations = totalConsults
        };
    }

    public async Task<IEnumerable<PatientTimelineItemDto>> GetClinicalTimelineAsync(Guid patientId, CancellationToken ct = default)
    {
        var timeline = new List<PatientTimelineItemDto>();

        // Visits
        var visits = await _unitOfWork.Visits.Query()
            .Include(v => v.Doctor)
            .Include(v => v.Department)
            .Where(v => v.PatientId == patientId)
            .ToListAsync(ct);

        foreach (var v in visits)
        {
            timeline.Add(new PatientTimelineItemDto
            {
                ReferenceId = v.Id,
                Date = v.CheckInTime,
                EventType = "Visit",
                Title = $"Visit - {v.VisitType}",
                Description = $"Consulted Dr. {v.Doctor.FullName} in {v.Department?.Name}. Status: {v.Status}"
            });
        }

        // Consultations
        var consults = await _unitOfWork.Consultations.Query()
            .Include(c => c.Doctor)
            .Include(c => c.Visit)
            .Where(c => c.Visit.PatientId == patientId)
            .ToListAsync(ct);

        foreach (var c in consults)
        {
            timeline.Add(new PatientTimelineItemDto
            {
                ReferenceId = c.Id,
                Date = c.CreatedAt,
                EventType = "Consultation",
                Title = "Consultation",
                Description = $"Diagnosis: {c.Diagnosis}. Status: {c.Status}"
            });
        }

        // Prescriptions
        var prescriptions = await _unitOfWork.Prescriptions.Query()
            .Include(p => p.Doctor)
            .Include(p => p.Items)
            .Where(p => p.PatientId == patientId)
            .ToListAsync(ct);

        foreach (var p in prescriptions)
        {
            timeline.Add(new PatientTimelineItemDto
            {
                ReferenceId = p.Id,
                Date = p.CreatedAt,
                EventType = "Prescription",
                Title = "Prescription Issued",
                Description = $"{p.Items.Count} medications prescribed by Dr. {p.Doctor.FullName}. Status: {p.Status}"
            });
        }

        // Lab Reports
        var labs = await _unitOfWork.LabReports.Query()
            .Where(l => l.PatientId == patientId)
            .ToListAsync(ct);

        foreach (var l in labs)
        {
            timeline.Add(new PatientTimelineItemDto
            {
                ReferenceId = l.Id,
                Date = l.CreatedAt,
                EventType = "LabReport",
                Title = "Lab Report",
                Description = $"{l.ReportName}. Status: {l.Status}"
            });
        }
        
        var emr = await _unitOfWork.EmrRecords.FirstOrDefaultAsync(e => e.PatientId == patientId, ct);
        if (emr != null)
        {
            // Immunizations
            var immunizations = await _unitOfWork.Immunizations.Query().Where(i => i.EmrRecordId == emr.Id).ToListAsync(ct);
            foreach (var i in immunizations)
            {
                timeline.Add(new PatientTimelineItemDto
                {
                    ReferenceId = i.Id,
                    Date = i.DateAdministered,
                    EventType = "Immunization",
                    Title = "Vaccination",
                    Description = $"{i.VaccineName} - Dose {i.DoseNumber}. Status: Completed"
                });
            }
        }

        return timeline.OrderByDescending(t => t.Date).ToList();
    }

    public async Task<VitalsDto?> GetLatestVitalsAsync(Guid patientId, CancellationToken ct = default)
    {
        var emr = await _unitOfWork.EmrRecords.FirstOrDefaultAsync(e => e.PatientId == patientId, ct);
        if (emr == null) return null;

        var latestVitals = await _unitOfWork.Vitals.Query()
            .Where(v => v.EmrRecordId == emr.Id)
            .OrderByDescending(v => v.RecordedAt)
            .FirstOrDefaultAsync(ct);

        if (latestVitals == null) return null;

        return new VitalsDto
        {
            Id = latestVitals.Id,
            VisitId = latestVitals.VisitId,
            RecordedAt = latestVitals.RecordedAt,
            HeartRate = latestVitals.HeartRate,
            BloodPressure = latestVitals.BloodPressure,
            Temperature = latestVitals.Temperature,
            RespiratoryRate = latestVitals.RespiratoryRate,
            O2Saturation = latestVitals.O2Saturation,
            Height = latestVitals.Height,
            Weight = latestVitals.Weight,
            CreatedAt = latestVitals.CreatedAt
        };
    }

    public async Task<IEnumerable<DiagnosisHistoryDto>> GetDiagnosisHistoryAsync(Guid patientId, CancellationToken ct = default)
    {
        var consults = await _unitOfWork.Consultations.Query()
            .Include(c => c.Doctor)
            .Include(c => c.Visit)
            .Where(c => c.Visit.PatientId == patientId && !string.IsNullOrEmpty(c.Diagnosis))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        return consults.Select(c => new DiagnosisHistoryDto
        {
            Diagnosis = c.Diagnosis!,
            IcdCode = c.DiagnosisCode,
            DiagnosedOn = c.CreatedAt,
            DiagnosedBy = $"Dr. {c.Doctor.FullName}",
            CurrentStatus = "Historical"
        }).ToList();
    }

    public async Task<EmergencyInfoDto> GetEmergencyInfoAsync(Guid patientId, CancellationToken ct = default)
    {
        var patient = await _unitOfWork.Patients.FirstOrDefaultAsync(p => p.Id == patientId, ct)
            ?? throw new NotFoundException("Patient not found.");

        return new EmergencyInfoDto
        {
            BloodGroup = patient.BloodGroup?.ToString(),
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone,
            EmergencyContactRelationship = patient.EmergencyContactRelationship,
            OrganDonorFlag = patient.OrganDonorFlag
        };
    }

    public async Task<EmergencyInfoDto> UpdateEmergencyInfoAsync(Guid patientId, UpdateEmergencyInfoRequestDto request, CancellationToken ct = default)
    {
        var patient = await _unitOfWork.Patients.FirstOrDefaultAsync(p => p.Id == patientId, ct)
            ?? throw new NotFoundException("Patient not found.");

        if (!string.IsNullOrEmpty(request.BloodGroup) && Enum.TryParse<HospitalManagement.DataAccess.Models.Enums.BloodGroup>(request.BloodGroup, true, out var bloodGroup))
        {
            patient.BloodGroup = bloodGroup;
        }

        patient.EmergencyContactName = request.EmergencyContactName;
        patient.EmergencyContactPhone = request.EmergencyContactPhone;
        patient.EmergencyContactRelationship = request.EmergencyContactRelationship;
        patient.OrganDonorFlag = request.OrganDonorFlag;

        await _unitOfWork.CompleteAsync(ct);

        return new EmergencyInfoDto
        {
            BloodGroup = patient.BloodGroup?.ToString(),
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone,
            EmergencyContactRelationship = patient.EmergencyContactRelationship,
            OrganDonorFlag = patient.OrganDonorFlag
        };
    }

    public async Task<ImmunizationDto> AddImmunizationAsync(Guid patientId, CreateImmunizationRequestDto request, CancellationToken ct = default)
    {
        var emr = await GetOrThrowEmrAsync(patientId, ct);

        var immunization = new Immunization
        {
            EmrRecordId = emr.Id,
            VaccineName = request.VaccineName,
            DateAdministered = request.DateAdministered,
            DoseNumber = request.DoseNumber,
            Provider = request.Provider,
            Notes = request.Notes
        };

        emr.Immunizations.Add(immunization);
        await _unitOfWork.CompleteAsync(ct);

        return new ImmunizationDto
        {
            Id = immunization.Id,
            VaccineName = immunization.VaccineName,
            DateAdministered = immunization.DateAdministered,
            DoseNumber = immunization.DoseNumber,
            Provider = immunization.Provider,
            Notes = immunization.Notes
        };
    }

    public async Task<IEnumerable<ImmunizationDto>> GetImmunizationsAsync(Guid patientId, CancellationToken ct = default)
    {
        var emr = await GetOrThrowEmrAsync(patientId, ct);
        
        var immunizations = await _unitOfWork.Immunizations.Query()
            .Where(i => i.EmrRecordId == emr.Id)
            .OrderByDescending(i => i.DateAdministered)
            .ToListAsync(ct);

        return immunizations.Select(i => new ImmunizationDto
        {
            Id = i.Id,
            VaccineName = i.VaccineName,
            DateAdministered = i.DateAdministered,
            DoseNumber = i.DoseNumber,
            Provider = i.Provider,
            Notes = i.Notes
        }).ToList();
    }

    public async Task UpdateImmunizationAsync(Guid patientId, Guid immunizationId, UpdateImmunizationRequestDto request, CancellationToken ct = default)
    {
        var immunization = await _unitOfWork.Immunizations.Query()
            .Include(i => i.EmrRecord)
            .FirstOrDefaultAsync(i => i.Id == immunizationId && i.EmrRecord.PatientId == patientId, ct)
            ?? throw new NotFoundException("Immunization not found.");

        immunization.VaccineName = request.VaccineName;
        immunization.DateAdministered = request.DateAdministered;
        immunization.DoseNumber = request.DoseNumber;
        immunization.Provider = request.Provider;
        immunization.Notes = request.Notes;

        await _unitOfWork.CompleteAsync(ct);
    }

    public async Task DeleteImmunizationAsync(Guid patientId, Guid immunizationId, CancellationToken ct = default)
    {
        var immunization = await _unitOfWork.Immunizations.Query()
            .Include(i => i.EmrRecord)
            .FirstOrDefaultAsync(i => i.Id == immunizationId && i.EmrRecord.PatientId == patientId, ct)
            ?? throw new NotFoundException("Immunization not found.");

        immunization.IsDeleted = true;
        immunization.DeletedAt = DateTime.UtcNow;

        await _unitOfWork.CompleteAsync(ct);
    }

    public async Task<EmrDocumentDto> UploadDocumentAsync(Guid patientId, UploadEmrDocumentRequestDto request, Guid uploadedBy, CancellationToken ct = default)
    {
        var emr = await GetOrThrowEmrAsync(patientId, ct);

        // Dummy blob URL generation - in real life, this goes to Azure Blob Storage
        var blobUrl = $"https://emrstorage.blob.core.windows.net/documents/{Guid.NewGuid()}_{request.File.FileName}";

        var document = new EmrDocument
        {
            EmrRecordId = emr.Id,
            BlobUrl = blobUrl,
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            UploadedBy = uploadedBy
        };

        emr.Documents.Add(document);
        await _unitOfWork.CompleteAsync(ct);

        return new EmrDocumentDto
        {
            Id = document.Id,
            BlobUrl = document.BlobUrl,
            FileName = document.FileName,
            ContentType = document.ContentType,
            UploadedBy = document.UploadedBy,
            UploadedAt = document.CreatedAt
        };
    }

    public async Task<IEnumerable<EmrDocumentDto>> GetDocumentsAsync(Guid patientId, CancellationToken ct = default)
    {
        var emr = await GetOrThrowEmrAsync(patientId, ct);
        
        var documents = await _unitOfWork.EmrDocuments.Query()
            .Where(d => d.EmrRecordId == emr.Id)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

        return documents.Select(d => new EmrDocumentDto
        {
            Id = d.Id,
            BlobUrl = d.BlobUrl,
            FileName = d.FileName,
            ContentType = d.ContentType,
            UploadedBy = d.UploadedBy,
            UploadedAt = d.CreatedAt
        }).ToList();
    }

    public async Task DeleteDocumentAsync(Guid patientId, Guid documentId, CancellationToken ct = default)
    {
        var document = await _unitOfWork.EmrDocuments.Query()
            .Include(d => d.EmrRecord)
            .FirstOrDefaultAsync(d => d.Id == documentId && d.EmrRecord.PatientId == patientId, ct)
            ?? throw new NotFoundException("Document not found.");

        document.IsDeleted = true;
        document.DeletedAt = DateTime.UtcNow;

        await _unitOfWork.CompleteAsync(ct);
    }

    public async Task<IEnumerable<EmrAuditDto>> GetAuditLogAsync(Guid patientId, CancellationToken ct = default)
    {
        // For audit, we'd typically query the actual AuditLogs table for records belonging to the patient's EMR.
        // EMR records, Allergy, Vitals, etc.
        var emr = await GetOrThrowEmrAsync(patientId, ct);

        var entityNames = new[] { "EmrRecord", "Allergy", "MedicalHistory", "Vitals", "Immunization", "EmrDocument" };
        var auditLogs = await _unitOfWork.AuditLogs
            .Where(a => entityNames.Contains(a.EntityName))
            .OrderByDescending(a => a.Timestamp)
            .Take(50) // Basic filter, in real app would filter by specific RecordIds 
            .ToListAsync(ct);

        // Since linking generic AuditLogs to a specific patient is complex without a robust structured log, 
        // we'll return a mapped DTO for demonstration of the endpoint.
        return auditLogs.Select(a => new EmrAuditDto
        {
            UserId = a.UserId ?? Guid.Empty,
            UserName = "System User", // Would join with Users
            Action = a.Action.ToString(),
            Timestamp = a.Timestamp,
            EntityType = a.EntityName,
            EntityId = a.RecordId
        }).ToList();
    }
}
