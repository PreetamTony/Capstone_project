using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Visit;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConsultationsController : ControllerBase
{
    private readonly IConsultationService _consultationService;

    public ConsultationsController(IConsultationService consultationService)
    {
        _consultationService = consultationService;
    }

    [HttpPost]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(ConsultationDetailsDto), 201)]
    public async Task<IActionResult> CreateConsultation([FromBody] CreateConsultationRequestDto request, CancellationToken ct)
    {
        var result = await _consultationService.CreateConsultationAsync(request, ct);
        return CreatedAtAction(nameof(GetConsultationById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ConsultationDetailsDto), 200)]
    public async Task<IActionResult> GetConsultationById(Guid id, CancellationToken ct)
    {
        var result = await _consultationService.GetConsultationByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("visit/{visitId}")]
    [ProducesResponseType(typeof(ConsultationDetailsDto), 200)]
    public async Task<IActionResult> GetConsultationByVisitId(Guid visitId, CancellationToken ct)
    {
        var result = await _consultationService.GetConsultationByVisitIdAsync(visitId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(ConsultationDetailsDto), 200)]
    public async Task<IActionResult> UpdateConsultation(Guid id, [FromBody] UpdateConsultationRequestDto request, CancellationToken ct)
    {
        var result = await _consultationService.UpdateConsultationAsync(id, request, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id}/complete")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(ConsultationDetailsDto), 200)]
    public async Task<IActionResult> CompleteConsultation(Guid id, CancellationToken ct)
    {
        var result = await _consultationService.CompleteConsultationAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(ConsultationDetailsDto), 200)]
    public async Task<IActionResult> CancelConsultation(Guid id, [FromBody] CancelVisitRequestDto request, CancellationToken ct)
    {
        var result = await _consultationService.CancelConsultationAsync(id, request.Reason ?? "Cancelled", ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Doctor}")]
    [ProducesResponseType(typeof(PagedResult<ConsultationSummaryDto>), 200)]
    public async Task<IActionResult> GetAllConsultations([FromQuery] ConsultationFilterDto filter, CancellationToken ct)
    {
        var result = await _consultationService.GetConsultationsAsync(filter, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("doctor/me")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(PagedResult<ConsultationSummaryDto>), 200)]
    public async Task<IActionResult> GetMyConsultationsDoctor([FromQuery] ConsultationFilterDto filter, CancellationToken ct)
    {
        filter.DoctorId = GetCurrentUserId();
        var result = await _consultationService.GetConsultationsAsync(filter, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("patient/me")]
    [Authorize(Roles = AppConstants.Roles.Patient)]
    [ProducesResponseType(typeof(PagedResult<ConsultationSummaryDto>), 200)]
    public async Task<IActionResult> GetMyConsultationsPatient([FromQuery] ConsultationFilterDto filter, CancellationToken ct)
    {
        filter.PatientId = GetCurrentUserId();
        var result = await _consultationService.GetConsultationsAsync(filter, ct);
        return Ok(new { success = true, data = result });
    }

    private Guid GetCurrentUserId()
    {
        var id = User.FindFirstValue(AppConstants.Jwt.ClaimUserId)
                 ?? throw new HospitalManagement.DataAccess.Exceptions.UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(id);
    }
}
