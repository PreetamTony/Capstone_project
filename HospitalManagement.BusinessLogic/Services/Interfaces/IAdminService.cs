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
}
