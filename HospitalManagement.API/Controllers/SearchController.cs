using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Doctor;
using HospitalManagement.BusinessLogic.DTOs.Patient;
using HospitalManagement.DataAccess.Repositories;
using HospitalManagement.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalManagement.BusinessLogic.DTOs.Appointment;
using HospitalManagement.BusinessLogic.DTOs.Visit;

namespace HospitalManagement.Presentation.Controllers;

[ApiController]
[Route("api/search")]
[Authorize]
[Produces("application/json")]
public class SearchController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public SearchController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet("doctors")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchDoctors(
        [FromQuery] string? query, 
        [FromQuery] string? specialization, 
        [FromQuery] int? minExperience, 
        [FromQuery] int? maxExperience, 
        [FromQuery] string? language, 
        CancellationToken ct)
    {
        var q = _uow.Doctors.Query()
            .Include(d => d.Department)
            .Include(d => d.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.ToLower();
            q = q.Where(d => d.FirstName.ToLower().Contains(query) || 
                             d.LastName.ToLower().Contains(query) ||
                             d.Specialization.ToLower().Contains(query));
        }

        if (!string.IsNullOrWhiteSpace(specialization))
        {
            specialization = specialization.ToLower();
            q = q.Where(d => d.Specialization.ToLower() == specialization);
        }

        if (minExperience.HasValue)
        {
            q = q.Where(d => d.ExperienceYears >= minExperience.Value);
        }

        if (maxExperience.HasValue)
        {
            q = q.Where(d => d.ExperienceYears <= maxExperience.Value);
        }

        if (!string.IsNullOrWhiteSpace(language))
        {
            language = language.ToLower();
            q = q.AsEnumerable().Where(d => d.Languages.Any(l => l.ToLower() == language)).AsQueryable();
        }

        var doctors = await q.ToListAsync(ct);

        // Fetch schedules for availability
        var today = (int)DateTime.UtcNow.DayOfWeek;
        var currentDay = today == 0 ? 7 : today;
        var doctorIds = doctors.Select(d => d.Id).ToList();
        
        var schedules = await _uow.DoctorSchedules.Query()
            .Where(s => doctorIds.Contains(s.DoctorId) && s.DayOfWeek == currentDay)
            .ToListAsync(ct);

        var results = doctors.Select(d => new DoctorResponseDto
        {
            Id = d.Id,
            UserId = d.UserId,
            FullName = $"{d.FirstName} {d.LastName}",
            FirstName = d.FirstName,
            LastName = d.LastName,
            Specialization = d.Specialization,
            DepartmentId = d.DepartmentId,
            DepartmentName = d.Department != null ? d.Department.Name : string.Empty,
            LicenseNumber = d.LicenseNumber,
            YearsOfExperience = d.ExperienceYears,
            ConsultationFee = d.ConsultationFee,
            MaxPatientsPerDay = d.MaxPatientsPerDay,
            AverageConsultationMinutes = d.AverageConsultationMinutes,
            IsAvailable = schedules.Any(s => s.DoctorId == d.Id),
            Rating = d.Rating,
            Email = d.User != null ? d.User.Email : string.Empty,
            PhoneNumber = d.User != null ? d.User.PhoneNumber : string.Empty
        }).ToList();

        return Ok(new { success = true, data = results });
    }

    [HttpGet("patients")]
    public async Task<IActionResult> SearchPatients([FromQuery] string? query, CancellationToken ct)
    {
        var q = _uow.Patients.Query()
            .Include(p => p.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.ToLower();
            q = q.Where(p => p.FirstName.ToLower().Contains(query) || 
                             p.LastName.ToLower().Contains(query) ||
                             (p.User != null && p.User.PhoneNumber.Contains(query)) ||
                             p.EmergencyContactPhone.Contains(query));
        }

        var results = await q.Select(p => new PatientResponseDto
        {
            Id = p.Id,
            UserId = p.UserId,
            FirstName = p.FirstName,
            LastName = p.LastName,
            FullName = p.FullName,
            Age = p.Age,
            DateOfBirth = p.DateOfBirth,
            Gender = p.Gender.ToString(),
            BloodGroup = p.BloodGroup.ToString(),
            Address = p.Address,
            Email = p.User != null ? p.User.Email : string.Empty,
            PhoneNumber = p.User != null ? p.User.PhoneNumber : string.Empty,
            EmergencyContactName = p.EmergencyContactName,
            EmergencyContactPhone = p.EmergencyContactPhone,
            InsuranceProvider = p.InsuranceProvider,
            InsurancePolicyNumber = p.InsurancePolicyNumber,
            InsuranceCoveragePercent = p.InsuranceCoveragePercent,
            NoShowCount = p.NoShowCount,
            IsPriority = p.IsPriority,
            CreatedAt = p.CreatedAt
        }).ToListAsync(ct);

        return Ok(new { success = true, data = results });
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> SearchAppointments([FromQuery] string? patientName, [FromQuery] DateTime? date, CancellationToken ct)
    {
        var q = _uow.Appointments.Query()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .AsQueryable();

        if (date.HasValue)
        {
            q = q.Where(a => a.AppointmentTime.Date == date.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(patientName))
        {
            patientName = patientName.ToLower();
            q = q.Where(a => a.Patient != null && (a.Patient.FirstName.ToLower().Contains(patientName) || a.Patient.LastName.ToLower().Contains(patientName)));
        }

        var results = await q.Select(a => new AppointmentResponseDto
        {
            Id = a.Id,
            PatientId = a.PatientId,
            DoctorId = a.DoctorId,
            PatientName = a.Patient != null ? $"{a.Patient.FirstName} {a.Patient.LastName}" : string.Empty,
            DoctorName = a.Doctor != null ? $"Dr. {a.Doctor.FirstName} {a.Doctor.LastName}" : string.Empty,
            AppointmentTime = a.AppointmentTime,
            EndTime = a.EndTime,
            Status = a.Status.ToString(),
            Type = a.Type.ToString(),
            Reason = a.Reason,
            Priority = a.Priority.ToString(),
            CancellationReason = a.CancellationReason,
            ReminderSent = a.ReminderSent,
            LateCancellationPenalty = a.LateCancellationPenalty,
            CheckedInAt = a.CheckedInAt,
            CompletedAt = a.CompletedAt,
            CreatedAt = a.CreatedAt
        }).ToListAsync(ct);

        return Ok(new { success = true, data = results });
    }
}
