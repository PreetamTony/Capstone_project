using HospitalManagement.BusinessLogic.DTOs.Visit;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IConsultationService
{
    Task<ConsultationResponseDto> CreateConsultationAsync(CreateConsultationRequestDto request, CancellationToken ct = default);
    Task<ConsultationResponseDto> GetConsultationByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ConsultationResponseDto>> GetConsultationsByVisitIdAsync(Guid visitId, CancellationToken ct = default);
    Task<ConsultationResponseDto> UpdateConsultationAsync(Guid id, CreateConsultationRequestDto request, CancellationToken ct = default);
}
