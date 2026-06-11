using HospitalManagement.BusinessLogic.DTOs.Patient;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Presentation.Controllers;

[ApiController]
[Route("api/patients/{patientId:guid}/timeline")]
[Authorize]
[Produces("application/json")]
public class TimelineController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public TimelineController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<PatientTimelineItemDto>), 200)]
    public async Task<IActionResult> GetTimeline(Guid patientId, CancellationToken ct)
    {
        var timeline = new List<PatientTimelineItemDto>();

        // 1. Appointments
        var appointments = await _uow.Appointments.Query()
            .Where(a => a.PatientId == patientId)
            .Select(a => new PatientTimelineItemDto
            {
                Date = a.CreatedAt,
                EventType = "Appointment",
                Title = "Appointment Booked",
                Description = $"Appointment status is {a.Status}.",
                ReferenceId = a.Id
            }).ToListAsync(ct);

        // 2. Visits
        var visits = await _uow.Visits.Query()
            .Where(v => v.PatientId == patientId)
            .Select(v => new PatientTimelineItemDto
            {
                Date = v.CreatedAt,
                EventType = "Visit",
                Title = "Visit Completed",
                Description = $"Consulted with Doctor ID {v.DoctorId}",
                ReferenceId = v.Id
            }).ToListAsync(ct);

        // 3. Prescriptions
        var prescriptions = await _uow.Prescriptions.Query()
            .Where(p => p.PatientId == patientId)
            .Select(p => new PatientTimelineItemDto
            {
                Date = p.CreatedAt,
                EventType = "Prescription",
                Title = "Prescription Issued",
                Description = $"Prescribed {p.Items.Count} medication(s)",
                ReferenceId = p.Id
            }).ToListAsync(ct);

        // 4. Lab Reports
        var labReports = await _uow.LabReports.Query()
            .Where(l => l.PatientId == patientId)
            .Select(l => new PatientTimelineItemDto
            {
                Date = l.CreatedAt,
                EventType = "LabReport",
                Title = "Lab Report Uploaded",
                Description = $"Report: {l.ReportName}",
                ReferenceId = l.Id
            }).ToListAsync(ct);

        // 5. Bills
        var bills = await _uow.Invoices.Query()
            .Where(b => b.PatientId == patientId)
            .Select(b => new PatientTimelineItemDto
            {
                Date = b.CreatedAt,
                EventType = "Invoice",
                Title = "Invoice Generated",
                Description = $"Amount: {b.TotalAmount}, Status: {b.Status}",
                ReferenceId = b.Id
            }).ToListAsync(ct);

        timeline.AddRange(appointments);
        timeline.AddRange(visits);
        timeline.AddRange(prescriptions);
        timeline.AddRange(labReports);
        timeline.AddRange(bills);

        var sortedTimeline = timeline.OrderByDescending(t => t.Date).ToList();

        return Ok(new { success = true, data = sortedTimeline });
    }
}
