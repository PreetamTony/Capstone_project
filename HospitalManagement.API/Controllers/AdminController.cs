using HospitalManagement.BusinessLogic.DTOs.Admin;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using HospitalManagement.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

/// <summary>Admin-only endpoints — audit logs, daily summary, user management.</summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = AppConstants.Roles.Admin)]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    /// <summary>Get paginated audit logs with optional filters.</summary>
    [HttpGet("audit-logs")]
    [Authorize(Policy = AppConstants.Policies.ViewAuditLogs)]
    [ProducesResponseType(typeof(PagedResult<AuditLogResponseDto>), 200)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] Guid? userId = null,
        [FromQuery] string? entityName = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _adminService.GetAuditLogsAsync(userId, entityName, action, fromDate, toDate, search, pageNumber, pageSize, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get audit log details including old and new values.</summary>
    [HttpGet("audit-logs/{id:long}")]
    [Authorize(Policy = AppConstants.Policies.ViewAuditLogs)]
    [ProducesResponseType(typeof(AuditLogDetailResponseDto), 200)]
    public async Task<IActionResult> GetAuditLogById(long id, CancellationToken ct = default)
    {
        var result = await _adminService.GetAuditLogByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get a daily operations summary.</summary>
    [HttpGet("daily-summary")]
    [ProducesResponseType(typeof(DailySummaryDto), 200)]
    public async Task<IActionResult> GetDailySummary([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, CancellationToken ct = default)
    {
        var result = await _adminService.GetDailySummaryAsync(fromDate, toDate, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get all users with optional role filter.</summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResult<UserSummaryDto>), 200)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _adminService.GetUsersAsync(role, isActive, search, pageNumber, pageSize, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Create a new staff user (Doctor, Receptionist, Admin, etc.).</summary>
    [HttpPost("users")]
    [ProducesResponseType(typeof(CreateUserResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request, CancellationToken ct)
    {
        var result = await _adminService.CreateUserAsync(request, ct);
        return Created("", new { success = true, data = result });
    }

    /// <summary>Get user by Id.</summary>
    [HttpGet("users/{id:guid}")]
    [ProducesResponseType(typeof(UserSummaryDto), 200)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct)
    {
        var result = await _adminService.GetUserByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Update user's email.</summary>
    [HttpPut("users/{id:guid}/email")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> UpdateUserEmail(Guid id, [FromBody] UpdateUserEmailRequestDto request, CancellationToken ct)
    {
        await _adminService.UpdateUserEmailAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>Reset user's password to a temporary one.</summary>
    [HttpPut("users/{id:guid}/password-reset")]
    [ProducesResponseType(typeof(ResetPasswordResponseDto), 200)]
    public async Task<IActionResult> ResetUserPassword(Guid id, CancellationToken ct)
    {
        var result = await _adminService.ResetUserPasswordAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Export users to CSV.</summary>
    [HttpGet("users/export")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> ExportUsersCsv(CancellationToken ct)
    {
        var csvBytes = await _adminService.ExportUsersCsvAsync(ct);
        return File(csvBytes, "text/csv", "users_export.csv");
    }

    /// <summary>Get user statistics.</summary>
    [HttpGet("users/stats")]
    [ProducesResponseType(typeof(UserStatsDto), 200)]
    public async Task<IActionResult> GetUserStats(CancellationToken ct)
    {
        var result = await _adminService.GetUserStatsAsync(ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Bulk create users.</summary>
    [HttpPost("users/bulk")]
    [ProducesResponseType(typeof(BulkCreateUsersResponseDto), 200)]
    public async Task<IActionResult> BulkCreateUsers([FromBody] BulkCreateUsersRequestDto request, CancellationToken ct)
    {
        var result = await _adminService.BulkCreateUsersAsync(request, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Update a user's details.</summary>
    [HttpPut("users/{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequestDto request, CancellationToken ct)
    {
        await _adminService.UpdateUserAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>Archive (Soft Delete) a user.</summary>
    [HttpPut("users/{id:guid}/archive")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> ArchiveUser(Guid id, CancellationToken ct)
    {
        await _adminService.ArchiveUserAsync(id, ct);
        return NoContent();
    }

    /// <summary>Update a user's role.</summary>
    [HttpPut("users/{id:guid}/role")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleRequestDto request, CancellationToken ct)
    {
        await _adminService.UpdateUserRoleAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>Activate a user account.</summary>
    [HttpPut("users/{id:guid}/activate")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> ActivateUser(Guid id, CancellationToken ct)
    {
        await _adminService.ActivateUserAsync(id, ct);
        return NoContent();
    }

    /// <summary>Deactivate a user account.</summary>
    [HttpPut("users/{id:guid}/deactivate")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeactivateUser(Guid id, CancellationToken ct)
    {
        await _adminService.DeactivateUserAsync(id, ct);
        return NoContent();
    }

    /// <summary>Create a doctor profile for an existing user.</summary>
    [HttpPost("doctors")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateDoctorProfile([FromBody] CreateDoctorProfileRequestDto request, CancellationToken ct)
    {
        var doctorId = await _adminService.CreateDoctorProfileAsync(request, ct);
        return Created("", new { success = true, doctorId });
    }
}
