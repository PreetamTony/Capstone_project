using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Doctor;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalManagement.Presentation.Controllers;

/// <summary>Doctor profile and availability management.</summary>
[ApiController]
[Route("api/doctors")]
[Authorize]
[Produces("application/json")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<DoctorsController> _logger;

    public DoctorsController(IDoctorService doctorService, ILogger<DoctorsController> logger)
    {
        _doctorService = doctorService;
        _logger = logger;
    }

    /// <summary>Get all available doctors (paginated).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<DoctorResponseDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] DoctorPaginationFilter filter, CancellationToken ct)
    {
        var result = await _doctorService.GetAllAsync(filter, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get a doctor by their ID.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DoctorResponseDto), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _doctorService.GetByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }
}
