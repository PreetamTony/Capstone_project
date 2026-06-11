using System.Security.Claims;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Visit;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

[ApiController]
[Route("api/visits")]
[Authorize]
[Produces("application/json")]
public class VisitsController : ControllerBase
{
    private readonly IVisitService _visitService;
    private readonly ILogger<VisitsController> _logger;

    public VisitsController(IVisitService visitService, ILogger<VisitsController> logger)
    {
        _visitService = visitService;
        _logger = logger;
    }

    [HttpPost("/api/appointments/{appointmentId:guid}/visit")]
    [Authorize(Roles = $"{AppConstants.Roles.Receptionist},{AppConstants.Roles.Admin},{AppConstants.Roles.Doctor}")]
    [ProducesResponseType(typeof(VisitDetailsDto), 201)]
    public async Task<IActionResult> StartVisit(Guid appointmentId, [FromBody] StartVisitRequestDto request, CancellationToken ct)
    {
        var result = await _visitService.StartVisitAsync(appointmentId, request, ct);
        return CreatedAtAction(nameof(GetVisitById), new { visitId = result.Id }, new { success = true, data = result });
    }

    [HttpPost("{visitId}/discharge")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Admin}")]
    [ProducesResponseType(typeof(VisitDetailsDto), 200)]
    public async Task<IActionResult> DischargeVisit(Guid visitId, CancellationToken ct)
    {
        var result = await _visitService.DischargeVisitAsync(visitId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{visitId}/cancel")]
    [Authorize(Roles = $"{AppConstants.Roles.Receptionist},{AppConstants.Roles.Admin}")]
    [ProducesResponseType(typeof(VisitDetailsDto), 200)]
    public async Task<IActionResult> CancelVisit(Guid visitId, [FromBody] CancelVisitRequestDto request, CancellationToken ct)
    {
        var result = await _visitService.CancelVisitAsync(visitId, request, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{visitId}")]
    [ProducesResponseType(typeof(VisitDetailsDto), 200)]
    public async Task<IActionResult> GetVisitById(Guid visitId, CancellationToken ct)
    {
        var result = await _visitService.GetVisitByIdAsync(visitId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("patient/me")]
    [Authorize(Roles = AppConstants.Roles.Patient)]
    [ProducesResponseType(typeof(PagedResult<VisitSummaryDto>), 200)]
    public async Task<IActionResult> GetMyVisitsPatient([FromQuery] VisitFilterDto filter, CancellationToken ct)
    {
        filter.PatientId = GetCurrentUserId();
        var result = await _visitService.GetVisitsAsync(filter, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("doctor/me")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(PagedResult<VisitSummaryDto>), 200)]
    public async Task<IActionResult> GetMyVisitsDoctor([FromQuery] VisitFilterDto filter, CancellationToken ct)
    {
        filter.DoctorId = GetCurrentUserId();
        var result = await _visitService.GetVisitsAsync(filter, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Receptionist}")]
    [ProducesResponseType(typeof(PagedResult<VisitSummaryDto>), 200)]
    public async Task<IActionResult> GetAllVisits([FromQuery] VisitFilterDto filter, CancellationToken ct)
    {
        var result = await _visitService.GetVisitsAsync(filter, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{visitId}/history")]
    [Authorize]
    [ProducesResponseType(typeof(List<VisitHistoryDto>), 200)]
    public async Task<IActionResult> GetVisitHistory(Guid visitId, CancellationToken ct)
    {
        var result = await _visitService.GetVisitHistoryAsync(visitId, ct);
        return Ok(new { success = true, data = result });
    }

    private Guid GetCurrentUserId()
    {
        var id = User.FindFirstValue(AppConstants.Jwt.ClaimUserId)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID claim not found.");
        return Guid.Parse(id);
    }
}
