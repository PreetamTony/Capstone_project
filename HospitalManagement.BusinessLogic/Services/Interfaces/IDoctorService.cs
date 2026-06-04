using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Doctor;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IDoctorService
{
    Task<PagedResult<DoctorResponseDto>> GetAllAsync(DoctorPaginationFilter filter, CancellationToken ct = default);
    Task<DoctorResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
}
