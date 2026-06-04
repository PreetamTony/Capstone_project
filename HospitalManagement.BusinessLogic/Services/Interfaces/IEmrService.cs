using HospitalManagement.BusinessLogic.DTOs.Emr;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IEmrService
{
    Task<EmrRecordDto> GetEmrByPatientIdAsync(Guid patientId, CancellationToken ct = default);
    Task<FullEmrResponseDto> GetFullEmrAsync(Guid patientId, CancellationToken ct = default);
    Task<EmrRecordDto> InitializeEmrAsync(Guid patientId, InitializeEmrRequestDto request, CancellationToken ct = default);
    
    Task<AllergyDto> AddAllergyAsync(Guid patientId, CreateAllergyRequestDto request, CancellationToken ct = default);
    Task<MedicalHistoryDto> AddMedicalHistoryAsync(Guid patientId, CreateMedicalHistoryRequestDto request, CancellationToken ct = default);
    Task<VitalsDto> AddVitalsAsync(Guid patientId, CreateVitalsRequestDto request, CancellationToken ct = default);
}
