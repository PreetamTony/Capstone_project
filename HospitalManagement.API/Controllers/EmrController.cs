using HospitalManagement.BusinessLogic.DTOs.Emr;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmrController : ControllerBase
{
    private readonly IEmrService _emrService;

    public EmrController(IEmrService emrService)
    {
        _emrService = emrService;
    }

    [HttpGet("patient/{patientId}")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Patient}")]
    public async Task<ActionResult<EmrRecordDto>> GetEmrByPatientId(Guid patientId, CancellationToken ct)
    {
        var result = await _emrService.GetEmrByPatientIdAsync(patientId, ct);
        return Ok(result);
    }

    [HttpGet("patient/{patientId}/full")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Patient}")]
    public async Task<ActionResult<FullEmrResponseDto>> GetFullEmr(Guid patientId, CancellationToken ct)
    {
        var result = await _emrService.GetFullEmrAsync(patientId, ct);
        return Ok(result);
    }

    [HttpPost("patient/{patientId:guid}/initialize")]
    public async Task<ActionResult<EmrRecordDto>> InitializeEmr(Guid patientId, [FromBody] InitializeEmrRequestDto request, CancellationToken ct)
    {
        var result = await _emrService.InitializeEmrAsync(patientId, request, ct);
        return CreatedAtAction(nameof(GetEmrByPatientId), new { patientId = result.PatientId }, result);
    }

    [HttpPost("patient/{patientId:guid}/allergies")]
    public async Task<ActionResult<AllergyDto>> AddAllergy(Guid patientId, [FromBody] CreateAllergyRequestDto request, CancellationToken ct)
    {
        var result = await _emrService.AddAllergyAsync(patientId, request, ct);
        return Ok(result);
    }

    [HttpPost("patient/{patientId:guid}/history")]
    public async Task<ActionResult<MedicalHistoryDto>> AddMedicalHistory(Guid patientId, [FromBody] CreateMedicalHistoryRequestDto request, CancellationToken ct)
    {
        var result = await _emrService.AddMedicalHistoryAsync(patientId, request, ct);
        return Ok(result);
    }

    [HttpPost("patient/{patientId:guid}/vitals")]
    public async Task<ActionResult<VitalsDto>> AddVitals(Guid patientId, [FromBody] CreateVitalsRequestDto request, CancellationToken ct)
    {
        var result = await _emrService.AddVitalsAsync(patientId, request, ct);
        return Ok(result);
    }

    [HttpGet("patient/{patientId:guid}/vitals/history")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Patient}")]
    public async Task<ActionResult<List<VitalsDto>>> GetVitalsHistory(Guid patientId, CancellationToken ct)
    {
        var emr = await _emrService.GetEmrByPatientIdAsync(patientId, ct);
        return Ok(emr.Vitals.OrderByDescending(v => v.RecordedAt).ToList());
    }
}
