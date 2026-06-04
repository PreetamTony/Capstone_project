using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Visit;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

[ApiController]
[Route("api/visits")]
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
    [Authorize(Roles = $"{AppConstants.Roles.Receptionist},{AppConstants.Roles.Admin}")]
    [ProducesResponseType(typeof(VisitDto), 201)]
    public async Task<IActionResult> StartVisit(Guid appointmentId, [FromBody] StartVisitRequestDto request, CancellationToken ct)
    {
        request.AppointmentId = appointmentId; // Ensure it matches route
        var result = await _visitService.StartVisitAsync(request, ct);
        return CreatedAtAction(nameof(GetVisitById), new { visitId = result.Id }, new { success = true, data = result });
    }

    [HttpPut("{visitId}")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor}")]
    [ProducesResponseType(typeof(VisitDto), 200)]
    public async Task<IActionResult> UpdateVisit(Guid visitId, [FromBody] UpdateVisitRequestDto request, CancellationToken ct)
    {
        var result = await _visitService.UpdateVisitAsync(visitId, request, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{visitId}/discharge")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Admin}")]
    [ProducesResponseType(typeof(VisitDto), 200)]
    public async Task<IActionResult> DischargeVisit(Guid visitId, CancellationToken ct)
    {
        var result = await _visitService.DischargeVisitAsync(visitId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{visitId}/cancel")]
    [Authorize(Roles = $"{AppConstants.Roles.Receptionist},{AppConstants.Roles.Admin}")]
    [ProducesResponseType(typeof(VisitDto), 200)]
    public async Task<IActionResult> CancelVisit(Guid visitId, CancellationToken ct)
    {
        var result = await _visitService.CancelVisitAsync(visitId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{visitId}")]
    [Authorize]
    [ProducesResponseType(typeof(VisitDto), 200)]
    public async Task<IActionResult> GetVisitById(Guid visitId, CancellationToken ct)
    {
        var result = await _visitService.GetVisitByIdAsync(visitId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("patient/{patientId}")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<VisitDto>), 200)]
    public async Task<IActionResult> GetVisitsByPatient(Guid patientId, [FromQuery] PaginationFilter filter, CancellationToken ct)
    {
        var result = await _visitService.GetVisitsByPatientAsync(patientId, filter, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("doctor/{doctorId}")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Admin}")]
    [ProducesResponseType(typeof(PagedResult<VisitDto>), 200)]
    public async Task<IActionResult> GetVisitsByDoctor(Guid doctorId, [FromQuery] PaginationFilter filter, CancellationToken ct)
    {
        var result = await _visitService.GetVisitsByDoctorAsync(doctorId, filter, ct);
        return Ok(new { success = true, data = result });
    }
}
