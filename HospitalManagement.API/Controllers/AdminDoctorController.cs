using HospitalManagement.BusinessLogic.DTOs.Admin;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

[ApiController]
[Route("api/admin/doctors")]
[Authorize(Roles = AppConstants.Roles.Admin)]
[Produces("application/json")]
public class AdminDoctorController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminDoctorController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDoctors(
        [FromQuery] Guid? departmentId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _adminService.GetDoctorsAsync(departmentId, isActive, search, pageNumber, pageSize, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDoctorById(Guid id, CancellationToken ct)
    {
        var result = await _adminService.GetDoctorByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateDoctor(Guid id, [FromBody] UpdateDoctorRequestDto request, CancellationToken ct)
    {
        await _adminService.UpdateDoctorAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> ArchiveDoctor(Guid id, CancellationToken ct)
    {
        await _adminService.ArchiveDoctorAsync(id, ct);
        return NoContent();
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportDoctorsCsv(CancellationToken ct)
    {
        var csvBytes = await _adminService.ExportDoctorsCsvAsync(ct);
        return File(csvBytes, "text/csv", "doctors_export.csv");
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDoctorStats(CancellationToken ct)
    {
        var result = await _adminService.GetDoctorStatsAsync(ct);
        return Ok(new { success = true, data = result });
    }

    // --- Leave Requests ---

    [HttpGet("leave-requests")]
    public async Task<IActionResult> GetLeaveRequests(
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? doctorId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _adminService.GetLeaveRequestsAsync(status, fromDate, toDate, doctorId, pageNumber, pageSize, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("leave-requests/{id:guid}")]
    public async Task<IActionResult> GetLeaveRequestById(Guid id, CancellationToken ct)
    {
        var result = await _adminService.GetLeaveRequestByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("leave-requests/{id:guid}/approve")]
    public async Task<IActionResult> ApproveLeaveRequest(Guid id, [FromBody] ApproveLeaveDto request, CancellationToken ct)
    {
        await _adminService.ApproveLeaveRequestAsync(id, request.Notes, ct);
        return Ok(new { success = true });
    }

    [HttpPost("leave-requests/{id:guid}/reject")]
    public async Task<IActionResult> RejectLeaveRequest(Guid id, [FromBody] RejectLeaveDto request, CancellationToken ct)
    {
        await _adminService.RejectLeaveRequestAsync(id, request.Reason, ct);
        return Ok(new { success = true });
    }

    [HttpPost("leave-requests/{id:guid}/cancel")]
    public async Task<IActionResult> CancelLeaveRequest(Guid id, CancellationToken ct)
    {
        await _adminService.CancelLeaveRequestAsync(id, ct);
        return Ok(new { success = true });
    }
}

public class ApproveLeaveDto { public string? ApprovedBy { get; set; } public string? Notes { get; set; } }
public class RejectLeaveDto { public string? RejectedBy { get; set; } public string Reason { get; set; } = string.Empty; }
