using HospitalManagement.BusinessLogic.DTOs.Admin;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using HospitalManagement.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HospitalManagement.BusinessLogic.Services; 

namespace HospitalManagement.Presentation.Controllers;

/// <summary>Admin-only endpoints — audit logs, daily summary, user management.</summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = AppConstants.Roles.Admin)]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IAuthService _authService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, IAuthService authService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _authService = authService;
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

    [HttpGet("daily-summary")]
    [ProducesResponseType(typeof(DailySummaryDto), 200)]
    public async Task<IActionResult> GetDailySummary([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, CancellationToken ct = default)
    {
        var result = await _adminService.GetDailySummaryAsync(fromDate, toDate, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("daily-summary/export")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> ExportDailySummary([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] string format = "csv", CancellationToken ct = default)
    {
        var summary = await _adminService.GetDailySummaryAsync(fromDate, toDate, ct);
        
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Metric,Value");
        sb.AppendLine($"Date Range,{summary.Date:yyyy-MM-dd}");
        sb.AppendLine($"Total Appointments,{summary.TotalAppointments}");
        sb.AppendLine($"Completed Appointments,{summary.CompletedAppointments}");
        sb.AppendLine($"Cancelled Appointments,{summary.CancelledAppointments}");
        sb.AppendLine($"No Shows,{summary.NoShows}");
        sb.AppendLine($"New Patients,{summary.NewPatients}");
        sb.AppendLine($"Returning Patients,{summary.ReturningPatients}");
        sb.AppendLine($"Walk-ins,{summary.WalkInPatients}");
        sb.AppendLine($"Teleconsultations,{summary.Teleconsultations}");
        sb.AppendLine($"Total Revenue,{summary.TotalRevenue}");
        sb.AppendLine($"Consultation Revenue,{summary.ConsultationRevenue}");
        sb.AppendLine($"Lab Revenue,{summary.LabRevenue}");
        sb.AppendLine($"Pharmacy Revenue,{summary.PharmacyRevenue}");
        sb.AppendLine($"Insurance Claims,{summary.InsuranceClaims}");
        sb.AppendLine($"Insurance Amount,{summary.InsuranceAmount}");
        sb.AppendLine($"Pending Payments,{summary.PendingPayments}");
        sb.AppendLine($"Refunds Processed,{summary.RefundsProcessed}");
        sb.AppendLine($"Total Doctors Available,{summary.TotalDoctorsAvailable}");
        sb.AppendLine($"Average Wait Time (Mins),{summary.AverageWaitTimeMinutes}");
        sb.AppendLine($"Average Consultation Time (Mins),{summary.AverageConsultationTimeMinutes}");
        sb.AppendLine($"Peak Hour,{summary.PeakHour}");
        sb.AppendLine($"Slowest Hour,{summary.SlowestHour}");

        return File(System.Text.Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"summary_{summary.Date:yyyyMMdd}.csv");
    }

    [HttpGet("daily-summary/compare")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> CompareDailySummary([FromQuery] DateTime date1, [FromQuery] DateTime date2, CancellationToken ct = default)
    {
        var summary1 = await _adminService.GetDailySummaryAsync(date1, date1, ct);
        var summary2 = await _adminService.GetDailySummaryAsync(date2, date2, ct);
        
        return Ok(new { 
            success = true, 
            data = new {
                Period1 = summary1,
                Period2 = summary2,
                Differences = new {
                    RevenueChange = summary2.TotalRevenue - summary1.TotalRevenue,
                    AppointmentsChange = summary2.TotalAppointments - summary1.TotalAppointments
                }
            } 
        });
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
    [ProducesResponseType(204)] //No content
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

    /// <summary>Get login history for a specific user.</summary>
    [HttpGet("users/{id:guid}/login-history")]
    public async Task<IActionResult> GetUserLoginHistory(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _authService.GetUserLoginHistoryAsync(id, pageNumber, pageSize, ct);
        return Ok(new { success = true, data = result });
    }
}
