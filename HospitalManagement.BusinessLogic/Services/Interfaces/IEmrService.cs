using HospitalManagement.BusinessLogic.DTOs.Emr;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IEmrService
{
    Task<EmrRecordDto> GetEmrByPatientIdAsync(Guid patientId, CancellationToken ct = default);
    Task<FullEmrResponseDto> GetFullEmrAsync(Guid patientId, CancellationToken ct = default);
    Task<EmrRecordDto> InitializeEmrAsync(Guid patientId, InitializeEmrRequestDto request, CancellationToken ct = default);
    
    Task<AllergyDto> AddAllergyAsync(Guid patientId, CreateAllergyRequestDto request, CancellationToken ct = default);
    Task UpdateAllergyAsync(Guid patientId, Guid allergyId, UpdateAllergyRequestDto request, CancellationToken ct = default);
    Task DeleteAllergyAsync(Guid patientId, Guid allergyId, CancellationToken ct = default);

    Task<MedicalHistoryDto> AddMedicalHistoryAsync(Guid patientId, CreateMedicalHistoryRequestDto request, CancellationToken ct = default);
    Task UpdateMedicalHistoryAsync(Guid patientId, Guid historyId, UpdateMedicalHistoryRequestDto request, CancellationToken ct = default);
    Task DeleteMedicalHistoryAsync(Guid patientId, Guid historyId, CancellationToken ct = default);

    Task<VitalsDto> AddVitalsAsync(Guid patientId, CreateVitalsRequestDto request, CancellationToken ct = default);

    Task<EmrSummaryDto> GetEmrSummaryAsync(Guid patientId, CancellationToken ct = default);
    Task<IEnumerable<HospitalManagement.BusinessLogic.DTOs.Patient.PatientTimelineItemDto>> GetClinicalTimelineAsync(Guid patientId, CancellationToken ct = default);
    Task<VitalsDto?> GetLatestVitalsAsync(Guid patientId, CancellationToken ct = default);
    Task<IEnumerable<DiagnosisHistoryDto>> GetDiagnosisHistoryAsync(Guid patientId, CancellationToken ct = default);

    Task<EmergencyInfoDto> GetEmergencyInfoAsync(Guid patientId, CancellationToken ct = default);
    Task<EmergencyInfoDto> UpdateEmergencyInfoAsync(Guid patientId, UpdateEmergencyInfoRequestDto request, CancellationToken ct = default);

    Task<ImmunizationDto> AddImmunizationAsync(Guid patientId, CreateImmunizationRequestDto request, CancellationToken ct = default);
    Task<IEnumerable<ImmunizationDto>> GetImmunizationsAsync(Guid patientId, CancellationToken ct = default);
    Task UpdateImmunizationAsync(Guid patientId, Guid immunizationId, UpdateImmunizationRequestDto request, CancellationToken ct = default);
    Task DeleteImmunizationAsync(Guid patientId, Guid immunizationId, CancellationToken ct = default);

    Task<EmrDocumentDto> UploadDocumentAsync(Guid patientId, UploadEmrDocumentRequestDto request, Guid uploadedBy, CancellationToken ct = default);
    Task<IEnumerable<EmrDocumentDto>> GetDocumentsAsync(Guid patientId, CancellationToken ct = default);
    Task DeleteDocumentAsync(Guid patientId, Guid documentId, CancellationToken ct = default);

    Task<IEnumerable<EmrAuditDto>> GetAuditLogAsync(Guid patientId, CancellationToken ct = default);
}
