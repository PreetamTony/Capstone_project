using HospitalManagement.BusinessLogic.DTOs.Patient;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

[ApiController]
[Route("api/patient-consents")]
[Authorize]
[Produces("application/json")]
public class PatientConsentsController : ControllerBase
{
    private readonly IPatientConsentService _consentService;

    public PatientConsentsController(IPatientConsentService consentService)
    {
        _consentService = consentService;
    }

    [HttpGet("{patientId:guid}")]
    [ProducesResponseType(typeof(List<PatientConsentResponseDto>), 200)]
    public async Task<IActionResult> GetByPatientId(Guid patientId, CancellationToken ct)
    {
        var result = await _consentService.GetConsentsByPatientIdAsync(patientId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{patientId:guid}")]
    [ProducesResponseType(typeof(PatientConsentResponseDto), 200)]
    public async Task<IActionResult> UpdateConsent(Guid patientId, [FromBody] UpdatePatientConsentRequestDto request, CancellationToken ct)
    {
        var result = await _consentService.UpdateConsentAsync(patientId, request, ct);
        return Ok(new { success = true, data = result });
    }
}
