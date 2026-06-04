using HospitalManagement.BusinessLogic.DTOs.Patient;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IPatientConsentService
{
    Task<List<PatientConsentResponseDto>> GetConsentsByPatientIdAsync(Guid patientId, CancellationToken ct = default);
    Task<PatientConsentResponseDto> UpdateConsentAsync(Guid patientId, UpdatePatientConsentRequestDto request, CancellationToken ct = default);
}
