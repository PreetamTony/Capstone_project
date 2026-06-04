using HospitalManagement.BusinessLogic.DTOs.Pharmacy;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Doctor}")]
public class PharmacyController : ControllerBase
{
    private readonly IPharmacyService _pharmacyService;

    public PharmacyController(IPharmacyService pharmacyService)
    {
        _pharmacyService = pharmacyService;
    }

    [HttpGet("inventory")]
    public async Task<ActionResult<List<MedicationInventoryDto>>> GetInventory(CancellationToken ct)
    {
        var result = await _pharmacyService.GetInventoryAsync(ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("dispense")]
    public async Task<ActionResult<DispensationRecordDto>> DispensePrescription([FromBody] DispensePrescriptionRequestDto request, CancellationToken ct)
    {
        var result = await _pharmacyService.DispensePrescriptionAsync(request, ct);
        return Ok(new { success = true, data = result });
    }
}
