using HospitalManagement.BusinessLogic.DTOs.Schedule;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public SchedulesController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<ActionResult<List<DoctorScheduleDto>>> GetDoctorSchedule(
        Guid doctorId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken ct)
    {
        var result = await _scheduleService.GetDoctorScheduleAsync(doctorId, startDate, endDate, ct);
        return Ok(result);
    }

    [HttpGet("doctor/{doctorId}/available-slots")]
    public async Task<ActionResult<List<TimeSlotDto>>> GetAvailableSlots(
        Guid doctorId, [FromQuery] DateTime date, CancellationToken ct)
    {
        var result = await _scheduleService.GetAvailableSlotsAsync(doctorId, date, ct);
        return Ok(result);
    }

    [HttpPost("doctor/{doctorId}/leave")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<IActionResult> ApplyLeave(Guid doctorId, [FromBody] ApplyLeaveRequestDto request, CancellationToken ct)
    {
        await _scheduleService.ApplyLeaveAsync(doctorId, request, ct);
        return NoContent();
    }

    [HttpPost("doctor/{doctorId}/block")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<IActionResult> BlockSlot(Guid doctorId, [FromBody] BlockSlotRequestDto request, CancellationToken ct)
    {
        await _scheduleService.BlockSlotAsync(doctorId, request, ct);
        return NoContent();
    }
}
