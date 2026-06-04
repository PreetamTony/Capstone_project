using HospitalManagement.BusinessLogic.DTOs.Patient;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IPatientService
{
    Task<PatientResponseDto> GetMyProfileAsync(Guid userId, CancellationToken ct = default);
    Task<PatientResponseDto> UpdateMyProfileAsync(Guid userId, UpdatePatientRequestDto request, CancellationToken ct = default);
    Task<PatientResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
}
