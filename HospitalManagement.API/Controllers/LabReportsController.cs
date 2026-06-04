using System.Security.Claims;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.LabReport;
using HospitalManagement.BusinessLogic.Services;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

/// <summary>Lab report upload, download, listing, and status update.</summary>
[ApiController]
[Route("api/lab-reports")]
[Authorize]
[Produces("application/json")]
public class LabReportsController : ControllerBase
{
    private readonly ILabReportService _labReportService;
    private readonly ILogger<LabReportsController> _logger;

    public LabReportsController(ILabReportService labReportService, ILogger<LabReportsController> logger)
    {
        _labReportService = labReportService;
        _logger = logger;
    }

    /// <summary>Upload a lab report file (LabTechnician or Doctor).</summary>
    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.LabTechnician},{AppConstants.Roles.Doctor},{AppConstants.Roles.Admin}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(LabReportResponseDto), 201)]
    public async Task<IActionResult> Upload([FromForm] UploadLabReportRequestDto request, CancellationToken ct)
    {
        var result = await _labReportService.UploadReportAsync(GetCurrentUserId(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    /// <summary>Get a lab report by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LabReportResponseDto), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _labReportService.GetByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Download the actual lab report file.</summary>
    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var (content, contentType, fileName) = await _labReportService.DownloadReportAsync(id, GetCurrentUserId(), ct);
        return File(content, contentType, fileName);
    }

    /// <summary>Get all lab reports for a patient.</summary>
    [HttpGet("patient/{patientId:guid}")]
    [ProducesResponseType(typeof(PagedResult<LabReportResponseDto>), 200)]
    public async Task<IActionResult> GetByPatient(Guid patientId, [FromQuery] PaginationFilter filter, CancellationToken ct)
    {
        var result = await _labReportService.GetPatientReportsAsync(patientId, filter, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Update lab report status (LabTechnician/Doctor/Admin).</summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = $"{AppConstants.Roles.LabTechnician},{AppConstants.Roles.Doctor},{AppConstants.Roles.Admin}")]
    [ProducesResponseType(typeof(LabReportResponseDto), 200)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status, CancellationToken ct)
    {
        var result = await _labReportService.UpdateStatusAsync(id, status, ct);
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
