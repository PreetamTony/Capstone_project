using HospitalManagement.BusinessLogic.DTOs.Dashboard;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("admin")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<ActionResult<AdminDashboardDto>> GetAdminDashboard(CancellationToken ct)
    {
        var result = await _dashboardService.GetAdminDashboardAsync(ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("doctor")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<ActionResult<DoctorDashboardDto>> GetDoctorDashboard(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _dashboardService.GetDoctorDashboardAsync(userId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("patient")]
    [Authorize(Roles = AppConstants.Roles.Patient)]
    public async Task<ActionResult<PatientDashboardDto>> GetPatientDashboard(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _dashboardService.GetPatientDashboardAsync(userId, ct);
        return Ok(new { success = true, data = result });
    }

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
    }
}
