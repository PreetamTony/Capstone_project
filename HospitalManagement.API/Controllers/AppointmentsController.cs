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

    /// <summary>Book a new appointment (Patient or Receptionist).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AppointmentDetailsDto), 201)]
    public async Task<IActionResult> Book([FromBody] BookAppointmentRequestDto request, CancellationToken ct)
    {
        var result = await _appointmentService.BookAppointmentAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    /// <summary>Get appointment by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AppointmentDetailsDto), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _appointmentService.GetByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get available time slots for a doctor on a given date.</summary>
    [HttpGet("available-slots")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AvailableSlotDto), 200)]
    public async Task<IActionResult> GetAvailableSlots([FromQuery] Guid doctorId, [FromQuery] DateTime date, CancellationToken ct)
    {
        var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId, date, ct);
        return Ok(new { success = true, data = slots });
    }

    /// <summary>Get current patient's appointments (paginated & filtered).</summary>
    [HttpGet("patient/me")]
    [Authorize(Roles = AppConstants.Roles.Patient)]
    [ProducesResponseType(typeof(PagedResult<AppointmentSummaryDto>), 200)]
    public async Task<IActionResult> GetMyAppointments([FromQuery] AppointmentFilterDto filter, CancellationToken ct)
    {
        filter.PatientId = GetCurrentUserId(); // enforce patient ID to current user
        var result = await _appointmentService.GetAppointmentsAsync(filter, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get doctor's appointments (Doctor only).</summary>
    [HttpGet("doctor/me")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(PagedResult<AppointmentSummaryDto>), 200)]
    public async Task<IActionResult> GetDoctorAppointments([FromQuery] AppointmentFilterDto filter, CancellationToken ct)
    {
        filter.DoctorId = GetCurrentUserId(); // enforce doctor ID to current user
        var result = await _appointmentService.GetAppointmentsAsync(filter, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get all appointments (Receptionist/Admin).</summary>
    [HttpGet]
    [Authorize(Roles = $"{AppConstants.Roles.Receptionist},{AppConstants.Roles.Admin}")]
    [ProducesResponseType(typeof(PagedResult<AppointmentSummaryDto>), 200)]
    public async Task<IActionResult> GetAllAppointments([FromQuery] AppointmentFilterDto filter, CancellationToken ct)
    {
        var result = await _appointmentService.GetAppointmentsAsync(filter, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Cancel an appointment.</summary>
    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType(typeof(AppointmentDetailsDto), 200)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAppointmentRequestDto request, CancellationToken ct)
    {
        var result = await _appointmentService.CancelAppointmentAsync(id, request, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Reschedule an appointment to a new time.</summary>
    [HttpPut("{id:guid}/reschedule")]
    [ProducesResponseType(typeof(AppointmentDetailsDto), 200)]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleAppointmentRequestDto request, CancellationToken ct)
    {
        var result = await _appointmentService.RescheduleAppointmentAsync(id, request, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Confirm an appointment.</summary>
    [HttpPut("{id:guid}/confirm")]
    [Authorize(Roles = $"{AppConstants.Roles.Receptionist},{AppConstants.Roles.Admin}")]
    [ProducesResponseType(typeof(AppointmentDetailsDto), 200)]
    public async Task<IActionResult> ConfirmAppointment(Guid id, CancellationToken ct)
    {
        var result = await _appointmentService.ConfirmAppointmentAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Check-in a patient (Receptionist or Doctor).</summary>
    [HttpPut("{id:guid}/check-in")]
    [Authorize(Roles = $"{AppConstants.Roles.Receptionist},{AppConstants.Roles.Doctor},{AppConstants.Roles.Admin}")]
    [ProducesResponseType(typeof(AppointmentDetailsDto), 200)]
    public async Task<IActionResult> CheckIn(Guid id, CancellationToken ct)
    {
        var result = await _appointmentService.CheckInPatientAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Start appointment (Doctor only).</summary>
    [HttpPut("{id:guid}/start")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(AppointmentDetailsDto), 200)]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct)
    {
        var result = await _appointmentService.StartAppointmentAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Complete appointment and auto-generate bill (Doctor only).</summary>
    [HttpPut("{id:guid}/complete")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    [ProducesResponseType(typeof(AppointmentDetailsDto), 200)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        var result = await _appointmentService.CompleteAppointmentAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Mark appointment as no-show (Doctor/Receptionist).</summary>
    [HttpPut("{id:guid}/no-show")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Receptionist},{AppConstants.Roles.Admin}")]
    [ProducesResponseType(typeof(AppointmentDetailsDto), 200)]
    public async Task<IActionResult> MarkNoShow(Guid id, CancellationToken ct)
    {
        var result = await _appointmentService.MarkNoShowAsync(id, ct);
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
