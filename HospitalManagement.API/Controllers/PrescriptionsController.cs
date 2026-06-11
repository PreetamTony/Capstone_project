using System.Security.Claims;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Prescription;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using HospitalManagement.DataAccess.Repositories;
using HospitalManagement.DataAccess.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

[ApiController]
[Route("api/prescriptions")]
[Authorize]
[Produces("application/json")]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;
    private readonly IUnitOfWork _uow;

    public PrescriptionsController(IPrescriptionService prescriptionService, IUnitOfWork uow)
    {
        _prescriptionService = prescriptionService;
        _uow = uow;
    }

    [HttpPost]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(PrescriptionResponseDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreatePrescriptionRequestDto request, CancellationToken ct)
    {
        var result = await _prescriptionService.CreatePrescriptionAsync(GetCurrentUserId(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpPost("{id:guid}/items")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(PrescriptionItemDto), 201)]
    public async Task<IActionResult> AddMedication(Guid id, [FromBody] AddMedicationItemRequestDto request, CancellationToken ct)
    {
        var result = await _prescriptionService.AddMedicationItemAsync(id, GetCurrentUserId(), request, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPut("items/{itemId:guid}")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(PrescriptionItemDto), 200)]
    public async Task<IActionResult> UpdateMedication(Guid itemId, [FromBody] UpdateMedicationItemRequestDto request, CancellationToken ct)
    {
        var result = await _prescriptionService.UpdateMedicationItemAsync(itemId, GetCurrentUserId(), request, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("items/{itemId:guid}")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<IActionResult> DeleteMedication(Guid itemId, CancellationToken ct)
    {
        await _prescriptionService.DeleteMedicationItemAsync(itemId, GetCurrentUserId(), ct);
        return Ok(new { success = true, message = "Medication removed." });
    }

    [HttpPost("{id:guid}/finalize")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(PrescriptionResponseDto), 200)]
    public async Task<IActionResult> Finalize(Guid id, CancellationToken ct)
    {
        var result = await _prescriptionService.FinalizePrescriptionAsync(id, GetCurrentUserId(), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id:guid}/void")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(PrescriptionResponseDto), 200)]
    public async Task<IActionResult> Void(Guid id, [FromBody] VoidPrescriptionRequestDto request, CancellationToken ct)
    {
        var result = await _prescriptionService.VoidPrescriptionAsync(id, GetCurrentUserId(), request, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PrescriptionResponseDto), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _prescriptionService.GetByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("consultation/{consultationId:guid}")]
    [ProducesResponseType(typeof(List<PrescriptionSummaryDto>), 200)]
    public async Task<IActionResult> GetByConsultation(Guid consultationId, CancellationToken ct)
    {
        var result = await _prescriptionService.GetByConsultationAsync(consultationId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("patient/me")]
    [Authorize(Roles = AppConstants.Roles.Patient)]
    [ProducesResponseType(typeof(PagedResult<PrescriptionSummaryDto>), 200)]
    public async Task<IActionResult> GetMyPrescriptionsPatient([FromQuery] PaginationFilter filter, CancellationToken ct)
    {
        var result = await _prescriptionService.GetPatientPrescriptionsAsync(GetCurrentUserId(), filter, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("doctor/me")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(PagedResult<PrescriptionSummaryDto>), 200)]
    public async Task<IActionResult> GetMyPrescriptionsDoctor([FromQuery] PaginationFilter filter, CancellationToken ct)
    {
        var result = await _prescriptionService.GetDoctorPrescriptionsAsync(GetCurrentUserId(), filter, ct);
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
