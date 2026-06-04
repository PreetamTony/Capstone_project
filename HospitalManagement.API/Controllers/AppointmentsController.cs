using System.Security.Claims;
using HospitalManagement.BusinessLogic.DTOs.Appointment;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.Services;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

/// <summary>Full appointment lifecycle — booking, cancellation, rescheduling, status transitions.</summary>
[ApiController]
[Route("api/appointments")]
[Authorize]
[Produces("application/json")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(IAppointmentService appointmentService, ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    /// <summary>Book a new appointment (Patient only).</summary>
    [HttpPost]
    [Authorize(Roles = AppConstants.Roles.Patient)]
    [ProducesResponseType(typeof(AppointmentResponseDto), 201)]
    public async Task<IActionResult> Book([FromBody] BookAppointmentRequestDto request, CancellationToken ct)
    {
        var result = await _appointmentService.BookAppointmentAsync(GetCurrentUserId(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    /// <summary>Get appointment by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AppointmentResponseDto), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _appointmentService.GetByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get available time slots for a doctor on a given date.</summary>
    [HttpGet("available-slots")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<AvailableSlotResponseDto>), 200)]
    public async Task<IActionResult> GetAvailableSlots([FromQuery] Guid doctorId, [FromQuery] DateTime date, CancellationToken ct)
    {
        var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId, date, ct);
        return Ok(new { success = true, data = slots });
    }

    /// <summary>Get current patient's appointments (paginated).</summary>
    [HttpGet("patient/me")]
    [Authorize(Roles = AppConstants.Roles.Patient)]
    [ProducesResponseType(typeof(PagedResult<AppointmentResponseDto>), 200)]
    public async Task<IActionResult> GetMyAppointments([FromQuery] PaginationFilter filter, CancellationToken ct)
    {
        var result = await _appointmentService.GetPatientAppointmentsAsync(GetCurrentUserId(), filter, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get doctor's appointments (Doctor only).</summary>
    [HttpGet("doctor/me")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(PagedResult<AppointmentResponseDto>), 200)]
    public async Task<IActionResult> GetDoctorAppointments([FromQuery] PaginationFilter filter, CancellationToken ct)
    {
        var result = await _appointmentService.GetDoctorAppointmentsAsync(GetCurrentUserId(), filter, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Cancel an appointment.</summary>
    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType(typeof(AppointmentResponseDto), 200)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAppointmentRequestDto request, CancellationToken ct)
    {
        var result = await _appointmentService.CancelAppointmentAsync(id, GetCurrentUserId(), request, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Reschedule an appointment to a new time.</summary>
    [HttpPut("{id:guid}/reschedule")]
    [ProducesResponseType(typeof(AppointmentResponseDto), 200)]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleAppointmentRequestDto request, CancellationToken ct)
    {
        var result = await _appointmentService.RescheduleAppointmentAsync(id, GetCurrentUserId(), request, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Confirm an appointment.</summary>
    [HttpPut("{id:guid}/confirm")]
    [Authorize(Roles = $"{AppConstants.Roles.Receptionist},{AppConstants.Roles.Admin}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ConfirmAppointment(Guid id, CancellationToken ct)
    {
        await _appointmentService.ConfirmAppointmentAsync(id, ct);
        return NoContent();
    }

    /// <summary>Check-in a patient (Receptionist or Doctor).</summary>
    [HttpPut("{id:guid}/check-in")]
    [Authorize(Roles = $"{AppConstants.Roles.Receptionist},{AppConstants.Roles.Doctor},{AppConstants.Roles.Admin}")]
    [ProducesResponseType(typeof(AppointmentResponseDto), 200)]
    public async Task<IActionResult> CheckIn(Guid id, CancellationToken ct)
    {
        var result = await _appointmentService.CheckInPatientAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Start appointment (Doctor only).</summary>
    [HttpPut("{id:guid}/start")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(AppointmentResponseDto), 200)]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct)
    {
        var result = await _appointmentService.StartAppointmentAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Complete appointment and auto-generate bill (Doctor only).</summary>
    [HttpPut("{id:guid}/complete")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(AppointmentResponseDto), 200)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        var result = await _appointmentService.CompleteAppointmentAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Mark appointment as no-show (Doctor/Receptionist).</summary>
    [HttpPut("{id:guid}/no-show")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Receptionist},{AppConstants.Roles.Admin}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MarkNoShow(Guid id, CancellationToken ct)
    {
        await _appointmentService.MarkNoShowAsync(id, ct);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var id = User.FindFirstValue(AppConstants.Jwt.ClaimUserId)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID claim not found.");
        return Guid.Parse(id);
    }
}
