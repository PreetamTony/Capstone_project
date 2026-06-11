using HospitalManagement.BusinessLogic.DTOs.DoctorBot;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Admin}")]
public class DoctorBotController : ControllerBase
{
    private readonly IDoctorBotService _doctorBotService;

    public DoctorBotController(IDoctorBotService doctorBotService)
    {
        _doctorBotService = doctorBotService;
    }

    [HttpPost("emr/{patientId:guid}/query")]
    public async Task<ActionResult<DoctorBotResponseDto>> QueryEmr(Guid patientId, [FromBody] DoctorBotRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("Question cannot be empty.");
        }

        var result = await _doctorBotService.QueryEmrAsync(patientId, request.Question, ct);
        return Ok(result);
    }

    [HttpPost("document/{documentId:guid}/query")]
    public async Task<ActionResult<DoctorBotResponseDto>> QueryDocument(Guid documentId, [FromBody] DoctorBotRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("Question cannot be empty.");
        }

        var result = await _doctorBotService.QueryDocumentAsync(documentId, request.Question, ct);
        return Ok(result);
    }
}
