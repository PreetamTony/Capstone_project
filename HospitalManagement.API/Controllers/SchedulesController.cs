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

    [HttpPost("doctor/{doctorId}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Doctor}")]
    public async Task<ActionResult<List<DoctorScheduleDto>>> CreateDoctorSchedule(
        Guid doctorId, [FromBody] CreateScheduleRequestDto request, CancellationToken ct)
    {
        var result = await _scheduleService.CreateDoctorScheduleAsync(doctorId, request, ct);
        return CreatedAtAction(nameof(GetDoctorSchedule), new { doctorId = doctorId, startDate = request.ValidFrom, endDate = request.ValidTo }, result);
    }

    [HttpPut("doctor/{doctorId}/schedule/{scheduleId}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Doctor}")]
    public async Task<IActionResult> UpdateDoctorSchedule(
        Guid doctorId, Guid scheduleId, [FromBody] UpdateScheduleRequestDto request, CancellationToken ct)
    {
        await _scheduleService.UpdateDoctorScheduleAsync(doctorId, scheduleId, request, ct);
        return NoContent();
    }

    [HttpDelete("doctor/{doctorId}/schedule/{scheduleId}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Doctor}")]
    public async Task<IActionResult> DeleteDoctorSchedule(
        Guid doctorId, Guid scheduleId, CancellationToken ct)
    {
        await _scheduleService.DeleteDoctorScheduleAsync(doctorId, scheduleId, ct);
        return NoContent();
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
