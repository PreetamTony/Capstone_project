using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Visit;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IConsultationService
{
    Task<ConsultationDetailsDto> CreateConsultationAsync(CreateConsultationRequestDto request, CancellationToken ct = default);
    Task<ConsultationDetailsDto> GetConsultationByIdAsync(Guid id, CancellationToken ct = default);
    Task<ConsultationDetailsDto> GetConsultationByVisitIdAsync(Guid visitId, CancellationToken ct = default);
    Task<ConsultationDetailsDto> UpdateConsultationAsync(Guid id, UpdateConsultationRequestDto request, CancellationToken ct = default);
    Task<ConsultationDetailsDto> CompleteConsultationAsync(Guid id, CancellationToken ct = default);
    Task<ConsultationDetailsDto> CancelConsultationAsync(Guid id, string reason, CancellationToken ct = default);
    Task<PagedResult<ConsultationSummaryDto>> GetConsultationsAsync(ConsultationFilterDto filter, CancellationToken ct = default);
}
