using HospitalManagement.BusinessLogic.DTOs.Patient;
using HospitalManagement.BusinessLogic.Services;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalManagement.Presentation.Controllers;

/// <summary>Patient profile management with resource-based access control.</summary>
[ApiController]
[Route("api/patients")]
[Authorize]
[Produces("application/json")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IAuthService _authService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(IPatientService patientService, IAuthService authService, ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>Get the current patient's own profile.</summary>
    [HttpGet("me")]
    [Authorize(Roles = AppConstants.Roles.Patient)]
    [ProducesResponseType(typeof(PatientResponseDto), 200)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var result = await _patientService.GetMyProfileAsync(GetCurrentUserId(), ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Create a new patient (Receptionist or Admin).</summary>
    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Receptionist}")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreatePatient([FromBody] HospitalManagement.BusinessLogic.DTOs.Auth.RegisterRequestDto request, CancellationToken ct)
    {
        var userId = await _authService.RegisterAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = userId }, new { success = true, data = new { id = userId } });
    }

    /// <summary>Update the current patient's profile.</summary>
    [HttpPut("me")]
    [Authorize(Roles = AppConstants.Roles.Patient)]
    [ProducesResponseType(typeof(PatientResponseDto), 200)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdatePatientRequestDto request, CancellationToken ct)
    {
        var result = await _patientService.UpdateMyProfileAsync(GetCurrentUserId(), request, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get any patient by ID (Admin or Doctor).</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Doctor}")]
    [ProducesResponseType(typeof(PatientResponseDto), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _patientService.GetByIdAsync(id, ct);
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
