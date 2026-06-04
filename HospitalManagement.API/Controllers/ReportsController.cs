using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = AppConstants.Roles.Admin)]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken ct)
    {
        var result = await _reportService.GetRevenueReportAsync(startDate, endDate, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointmentReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken ct)
    {
        var result = await _reportService.GetAppointmentReportAsync(startDate, endDate, ct);
        return Ok(new { success = true, data = result });
    }
}
