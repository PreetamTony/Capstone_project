using System.Security.Claims;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Prescription;
using HospitalManagement.BusinessLogic.Services;
using HospitalManagement.DataAccess.Constants;
using HospitalManagement.DataAccess.Repositories;
using HospitalManagement.DataAccess.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

/// <summary>Prescription creation, update (30-min window), void, dispense, and retrieval.</summary>
[ApiController]
[Route("api/prescriptions")]
[Authorize]
[Produces("application/json")]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PrescriptionsController> _logger;

    public PrescriptionsController(IPrescriptionService prescriptionService, IUnitOfWork uow,
        ILogger<PrescriptionsController> logger)
    {
        _prescriptionService = prescriptionService;
        _uow = uow;
        _logger = logger;
    }

    /// <summary>Create a new prescription (Doctor only).</summary>
    [HttpPost]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(PrescriptionResponseDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreatePrescriptionRequestDto request, CancellationToken ct)
    {
        var result = await _prescriptionService.CreatePrescriptionAsync(GetCurrentUserId(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    /// <summary>Get a prescription by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PrescriptionResponseDto), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _prescriptionService.GetByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Update prescription within the 30-minute edit window (Doctor only).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(PrescriptionResponseDto), 200)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePrescriptionRequestDto request, CancellationToken ct)
    {
        var result = await _prescriptionService.UpdatePrescriptionAsync(id, GetCurrentUserId(), request, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Void a prescription with a reason (Doctor only). Cannot be deleted.</summary>
    [HttpPost("{id:guid}/void")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(PrescriptionResponseDto), 200)]
    public async Task<IActionResult> Void(Guid id, [FromBody] VoidPrescriptionRequestDto request, CancellationToken ct)
    {
        var result = await _prescriptionService.VoidPrescriptionAsync(id, GetCurrentUserId(), request, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Mark prescription as dispensed (Pharmacist only).</summary>
    [HttpPost("{id:guid}/dispense")]
    [Authorize(Roles = AppConstants.Roles.Pharmacist)]
    [ProducesResponseType(typeof(PrescriptionResponseDto), 200)]
    public async Task<IActionResult> Dispense(Guid id, CancellationToken ct)
    {
        var result = await _prescriptionService.MarkDispensedAsync(id, GetCurrentUserId(), ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get all prescriptions for a patient (Admin, Doctor, or own Patient).</summary>
    [HttpGet("patient/{patientId:guid}")]
    [ProducesResponseType(typeof(PagedResult<PrescriptionResponseDto>), 200)]
    public async Task<IActionResult> GetByPatient(Guid patientId, [FromQuery] PaginationFilter filter, CancellationToken ct)
    {
        // Resource-based check: patients can only see their own
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        if (role == AppConstants.Roles.Patient)
        {
            var patient = await _uow.Patients.FirstOrDefaultAsync(p => p.UserId == GetCurrentUserId(), ct)
                ?? throw new NotFoundException("Patient profile not found.");
             if (patient.Id != patientId)
                throw new HospitalManagement.DataAccess.Exceptions.UnauthorizedAccessException("You can only view your own prescriptions.");
        }

        var result = await _prescriptionService.GetPatientPrescriptionsAsync(patientId, filter, ct);
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
