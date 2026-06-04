using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Visit;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IVisitService
{
    Task<VisitDto> StartVisitAsync(StartVisitRequestDto request, CancellationToken ct = default);
    Task<VisitDto> UpdateVisitAsync(Guid visitId, UpdateVisitRequestDto request, CancellationToken ct = default);
    Task<VisitDto> DischargeVisitAsync(Guid visitId, CancellationToken ct = default);
    Task<VisitDto> CancelVisitAsync(Guid visitId, CancellationToken ct = default);
    Task<VisitDto> GetVisitByIdAsync(Guid visitId, CancellationToken ct = default);
    Task<PagedResult<VisitDto>> GetVisitsByPatientAsync(Guid patientId, PaginationFilter filter, CancellationToken ct = default);
    Task<PagedResult<VisitDto>> GetVisitsByDoctorAsync(Guid doctorId, PaginationFilter filter, CancellationToken ct = default);
}
