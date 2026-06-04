using HospitalManagement.BusinessLogic.DTOs.Department;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllDepartmentsAsync(CancellationToken ct = default);
    Task<DepartmentDto> GetDepartmentByIdAsync(Guid id, CancellationToken ct = default);
    Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequestDto request, CancellationToken ct = default);
    Task UpdateDepartmentAsync(Guid id, UpdateDepartmentRequestDto request, CancellationToken ct = default);
    Task<bool> DeleteDepartmentAsync(Guid id, CancellationToken ct = default);
}
