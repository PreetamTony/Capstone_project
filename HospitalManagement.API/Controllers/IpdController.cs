using HospitalManagement.BusinessLogic.DTOs.Ipd;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Doctor},{AppConstants.Roles.Receptionist}")]
public class IpdController : ControllerBase
{
    private readonly IIpdService _ipdService;

    public IpdController(IIpdService ipdService)
    {
        _ipdService = ipdService;
    }

    [HttpGet("wards")]
    public async Task<ActionResult<List<WardDto>>> GetAllWards(CancellationToken ct)
    {
        var result = await _ipdService.GetAllWardsAsync(ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("wards/{wardId}/beds/available")]
    public async Task<ActionResult<List<BedDto>>> GetAvailableBeds(Guid wardId, CancellationToken ct)
    {
        var result = await _ipdService.GetAvailableBedsAsync(wardId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("admit")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Doctor}")]
    public async Task<ActionResult<AdmissionRecordDto>> AdmitPatient([FromBody] AdmitPatientRequestDto request, CancellationToken ct)
    {
        var result = await _ipdService.AdmitPatientAsync(request, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("discharge/{admissionId}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Doctor}")]
    public async Task<ActionResult<AdmissionRecordDto>> DischargePatient(Guid admissionId, [FromBody] DischargePatientRequestDto request, CancellationToken ct)
    {
        var result = await _ipdService.DischargePatientAsync(admissionId, request, ct);
        return Ok(new { success = true, data = result });
    }
}
