using System.Security.Claims;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.LabReport;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

[ApiController]
[Route("api/lab-reports")]
[Authorize]
[Produces("application/json")]
public class LabReportsController : ControllerBase
{
    private readonly ILabReportService _labReportService;

    public LabReportsController(ILabReportService labReportService)
    {
        _labReportService = labReportService;
    }

    [HttpPost("/api/lab-orders")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(LabReportResponseDto), 201)]
    public async Task<IActionResult> CreateLabOrder([FromBody] CreateLabOrderRequestDto request, CancellationToken ct)
    {
        var result = await _labReportService.CreateLabOrderAsync(GetCurrentUserId(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LabReportResponseDto), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _labReportService.GetByIdAsync(id, GetCurrentUserId(), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("patient/me")]
    [Authorize(Roles = AppConstants.Roles.Patient)]
    [ProducesResponseType(typeof(PagedResult<LabReportResponseDto>), 200)]
    public async Task<IActionResult> GetMyPatientReports([FromQuery] PaginationFilter filter, CancellationToken ct)
    {
        var result = await _labReportService.GetPatientReportsAsync(GetCurrentUserId(), filter, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("doctor/me")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(PagedResult<LabReportResponseDto>), 200)]
    public async Task<IActionResult> GetMyDoctorReports([FromQuery] PaginationFilter filter, CancellationToken ct)
    {
        var result = await _labReportService.GetDoctorReportsAsync(GetCurrentUserId(), filter, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("consultation/{consultationId:guid}")]
    [ProducesResponseType(typeof(List<LabReportResponseDto>), 200)]
    public async Task<IActionResult> GetReportsForConsultation(Guid consultationId, CancellationToken ct)
    {
        var result = await _labReportService.GetConsultationReportsAsync(consultationId, GetCurrentUserId(), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    [ProducesResponseType(typeof(LabReportResponseDto), 200)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateLabReportStatusDto request, CancellationToken ct)
    {
        var result = await _labReportService.UpdateOrderStatusAsync(id, request, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPut("{id:guid}/upload")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(LabReportResponseDto), 200)]
    public async Task<IActionResult> UploadLabReport(Guid id, [FromForm] UploadLabReportRequestDto request, CancellationToken ct)
    {
        var result = await _labReportService.UploadLabReportAsync(id, GetCurrentUserId(), request, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPut("{id:guid}/review")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(LabReportResponseDto), 200)]
    public async Task<IActionResult> ReviewReport(Guid id, [FromBody] ReviewLabReportDto request, CancellationToken ct)
    {
        var result = await _labReportService.ReviewLabReportAsync(id, GetCurrentUserId(), request, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DownloadReport(Guid id, CancellationToken ct)
    {
        var (fileBytes, contentType, fileName) = await _labReportService.DownloadReportAsync(id, GetCurrentUserId(), ct);
        return File(fileBytes, contentType, fileName);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteLabReport(Guid id, CancellationToken ct)
    {
        await _labReportService.DeleteLabReportAsync(id, ct);
        return NoContent();
    }

    [HttpGet("statistics")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    [ProducesResponseType(typeof(LabReportStatisticsDto), 200)]
    public async Task<IActionResult> GetStatistics(CancellationToken ct)
    {
        var stats = await _labReportService.GetStatisticsAsync(ct);
        return Ok(new { success = true, data = stats });
    }

    private Guid GetCurrentUserId()
    {
        var id = User.FindFirstValue(AppConstants.Jwt.ClaimUserId)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID claim not found.");
        return Guid.Parse(id);
    }
}
