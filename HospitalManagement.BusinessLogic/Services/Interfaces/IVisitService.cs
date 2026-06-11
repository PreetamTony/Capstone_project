using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Visit;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IVisitService
{
    Task<VisitDetailsDto> StartVisitAsync(Guid appointmentId, StartVisitRequestDto request, CancellationToken ct = default);
    Task<VisitDetailsDto> DischargeVisitAsync(Guid visitId, CancellationToken ct = default);
    Task<VisitDetailsDto> CancelVisitAsync(Guid visitId, CancelVisitRequestDto request, CancellationToken ct = default);
    Task<VisitDetailsDto> GetVisitByIdAsync(Guid visitId, CancellationToken ct = default);
    Task<PagedResult<VisitSummaryDto>> GetVisitsAsync(VisitFilterDto filter, CancellationToken ct = default);
    Task<List<VisitHistoryDto>> GetVisitHistoryAsync(Guid visitId, CancellationToken ct = default);
}
