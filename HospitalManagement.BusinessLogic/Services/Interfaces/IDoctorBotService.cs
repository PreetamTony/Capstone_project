using HospitalManagement.BusinessLogic.DTOs.DoctorBot;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IDoctorBotService
{
    Task<DoctorBotResponseDto> QueryEmrAsync(Guid patientId, string question, CancellationToken ct = default);
    Task<DoctorBotResponseDto> QueryDocumentAsync(Guid documentId, string question, CancellationToken ct = default);
}
