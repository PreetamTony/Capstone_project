using HospitalManagement.BusinessLogic.DTOs.Visit;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

[ApiController]
[Route("api/consultations")]
[Authorize]
[Produces("application/json")]
public class ConsultationsController : ControllerBase
{
    private readonly IConsultationService _consultationService;

    public ConsultationsController(IConsultationService consultationService)
    {
        _consultationService = consultationService;
    }

    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor}")]
    [ProducesResponseType(typeof(ConsultationResponseDto), 201)]
    public async Task<IActionResult> CreateConsultation([FromBody] CreateConsultationRequestDto request, CancellationToken ct)
    {
        var result = await _consultationService.CreateConsultationAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConsultationResponseDto), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _consultationService.GetConsultationByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("visit/{visitId:guid}")]
    [ProducesResponseType(typeof(List<ConsultationResponseDto>), 200)]
    public async Task<IActionResult> GetByVisitId(Guid visitId, CancellationToken ct)
    {
        var result = await _consultationService.GetConsultationsByVisitIdAsync(visitId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor}")]
    [ProducesResponseType(typeof(ConsultationResponseDto), 200)]
    public async Task<IActionResult> UpdateConsultation(Guid id, [FromBody] CreateConsultationRequestDto request, CancellationToken ct)
    {
        var result = await _consultationService.UpdateConsultationAsync(id, request, ct);
        return Ok(new { success = true, data = result });
    }
}
