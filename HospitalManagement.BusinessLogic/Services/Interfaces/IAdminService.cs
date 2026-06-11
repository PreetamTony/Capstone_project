using HospitalManagement.BusinessLogic.DTOs.Admin;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.DataAccess.Models;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IAdminService
{
    Task<PagedResult<AuditLogResponseDto>> GetAuditLogsAsync(Guid? userId, string? entityName, string? action, DateTime? fromDate, DateTime? toDate, string? search, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default);
    Task<AuditLogDetailResponseDto> GetAuditLogByIdAsync(long id, CancellationToken ct = default);
    Task<DailySummaryDto> GetDailySummaryAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    Task<PagedResult<UserSummaryDto>> GetUsersAsync(string? role, bool? isActive, string? search, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default);
    Task<CreateUserResponseDto> CreateUserAsync(CreateUserRequestDto request, CancellationToken ct = default);
    Task UpdateUserAsync(Guid userId, UpdateUserRequestDto request, CancellationToken ct = default);
    Task ArchiveUserAsync(Guid userId, CancellationToken ct = default);
    Task UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequestDto request, CancellationToken ct = default);
    Task ActivateUserAsync(Guid userId, CancellationToken ct = default);
    Task DeactivateUserAsync(Guid userId, CancellationToken ct = default);
    Task<Guid> CreateDoctorProfileAsync(CreateDoctorProfileRequestDto request, CancellationToken ct = default);
    Task<UserSummaryDto> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task UpdateUserEmailAsync(Guid userId, UpdateUserEmailRequestDto request, CancellationToken ct = default);
    Task<ResetPasswordResponseDto> ResetUserPasswordAsync(Guid userId, CancellationToken ct = default);
    Task<byte[]> ExportUsersCsvAsync(CancellationToken ct = default);
    Task<UserStatsDto> GetUserStatsAsync(CancellationToken ct = default);
    Task<BulkCreateUsersResponseDto> BulkCreateUsersAsync(BulkCreateUsersRequestDto request, CancellationToken ct = default);

    Task<PagedResult<DoctorSummaryDto>> GetDoctorsAsync(Guid? departmentId, bool? isActive, string? search, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default);
    Task<DoctorDetailDto> GetDoctorByIdAsync(Guid doctorId, CancellationToken ct = default);
    Task UpdateDoctorAsync(Guid doctorId, UpdateDoctorRequestDto request, CancellationToken ct = default);
    Task ArchiveDoctorAsync(Guid doctorId, CancellationToken ct = default);
    Task<byte[]> ExportDoctorsCsvAsync(CancellationToken ct = default);
    Task<DoctorStatsDto> GetDoctorStatsAsync(CancellationToken ct = default);

    Task<PagedResult<LeaveRequestDto>> GetLeaveRequestsAsync(string? status, DateTime? fromDate, DateTime? toDate, Guid? doctorId, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default);
    Task<LeaveRequestDto> GetLeaveRequestByIdAsync(Guid id, CancellationToken ct = default);
    Task ApproveLeaveRequestAsync(Guid id, string? notes, CancellationToken ct = default);
    Task RejectLeaveRequestAsync(Guid id, string reason, CancellationToken ct = default);
    Task CancelLeaveRequestAsync(Guid id, CancellationToken ct = default);
}
