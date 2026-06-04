using System.Text.Json;
using HospitalManagement.BusinessLogic.DTOs.Appointment;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.DataAccess.Constants;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Repositories;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.BusinessLogic.Services;

/// <summary>
/// Appointment service — enforces all booking business rules and manages the full appointment lifecycle.
/// </summary>
public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _uow;

    private readonly IBillingService _billing;
    private readonly IScheduleService _scheduleService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(IUnitOfWork uow,
        IBillingService billing, IScheduleService scheduleService, 
        INotificationService notificationService, ILogger<AppointmentService> logger)
    {
        _uow = uow;
        _billing = billing;
        _scheduleService = scheduleService;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<AppointmentResponseDto> BookAppointmentAsync(
        Guid patientUserId, BookAppointmentRequestDto request, CancellationToken ct = default)
    {
        // Resolve patient
        var patient = await _uow.Patients.FirstOrDefaultAsync(p => p.UserId == patientUserId, ct)
            ?? throw new NotFoundException("Patient profile not found for this user.");

        // Resolve doctor
        var doctor = await _uow.Doctors.GetByIdAsync(request.DoctorId, ct)
            ?? throw new NotFoundException("Doctor", request.DoctorId);

        // Past booking guard
        if (request.AppointmentTime <= DateTime.UtcNow)
            throw new BusinessRuleViolationException("PastAppointment", "Cannot book an appointment in the past.");

        var endTime = request.AppointmentTime.AddMinutes(doctor.AverageConsultationMinutes);

        // Check availability via Schedule Engine
        var isAvailable = await _scheduleService.IsDoctorAvailableAsync(doctor.Id, request.AppointmentTime, endTime, ct);
        if (!isAvailable)
            throw new BusinessRuleViolationException("SlotUnavailable", "The selected time slot is not available. It may be blocked, outside schedule, or the doctor is on leave.");

        // Daily patient limit
        var date = request.AppointmentTime.Date;
        var todayCount = await _uow.Appointments.CountAsync(
            a => a.DoctorId == request.DoctorId
              && a.AppointmentTime.Date == date
              && a.Status != AppointmentStatus.Cancelled
              && a.Status != AppointmentStatus.NoShow, ct);

        if (todayCount >= doctor.MaxPatientsPerDay)
            throw new BusinessRuleViolationException("DailyLimitReached",
                $"Doctor has reached the maximum of {doctor.MaxPatientsPerDay} patients for this day.");

        var appointment = new Appointment
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            AppointmentTime = request.AppointmentTime,
            EndTime = endTime,
            Status = AppointmentStatus.Scheduled,
            Type = request.Type,
            Reason = request.Reason,
            SymptomsJson = request.Symptoms != null ? JsonSerializer.Serialize(request.Symptoms) : null,
            Priority = request.Priority
        };

        await _uow.Appointments.AddAsync(appointment, ct);
        await _uow.CompleteAsync(ct);


        _logger.LogInformation("Appointment {Id} booked for Patient {PatientId} with Doctor {DoctorId} at {Time}",
            appointment.Id, patient.Id, doctor.Id, appointment.AppointmentTime);

        // Notify patient
        await _notificationService.NotifyAppointmentBookedAsync(patient.Id, appointment.Id, ct);

        return await MapToResponseDtoAsync(appointment, patient, doctor);
    }

    /// <inheritdoc/>
    public async Task<List<AvailableSlotResponseDto>> GetAvailableSlotsAsync(
        Guid doctorId, DateTime date, CancellationToken ct = default)
    {
        var slots = await _scheduleService.GetAvailableSlotsAsync(doctorId, date, ct);
        
        return slots.Select(s => new AvailableSlotResponseDto
        {
            StartTime = date.Date.Add(s.StartTime),
            EndTime = date.Date.Add(s.EndTime),
            IsAvailable = s.IsAvailable
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<AppointmentResponseDto> CancelAppointmentAsync(
        Guid appointmentId, Guid cancelledByUserId, CancelAppointmentRequestDto request, CancellationToken ct = default)
    {
        var appointment = await GetAppointmentWithNavigationAsync(appointmentId, ct);

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new BusinessRuleViolationException("AlreadyCancelled", "This appointment is already cancelled.");

        if (appointment.Status == AppointmentStatus.Completed)
            throw new BusinessRuleViolationException("AlreadyCompleted", "Cannot cancel a completed appointment.");

        // 4-hour notice rule
        var hoursUntilAppointment = (appointment.AppointmentTime - DateTime.UtcNow).TotalHours;
        var latePenalty = hoursUntilAppointment < AppConstants.Appointment.CancellationNoticeHours;

        if (latePenalty)
            _logger.LogWarning("Late cancellation for appointment {Id} — penalty applied", appointmentId);

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = request.Reason;
        appointment.CancelledBy = cancelledByUserId;
        appointment.LateCancellationPenalty = latePenalty;

        _uow.Appointments.Update(appointment);
        await _uow.CompleteAsync(ct);


        return await MapToResponseDtoAsync(appointment, appointment.Patient, appointment.Doctor);
    }

    /// <inheritdoc/>
    public async Task<AppointmentResponseDto> RescheduleAppointmentAsync(
        Guid appointmentId, Guid userId, RescheduleAppointmentRequestDto request, CancellationToken ct = default)
    {
        var appointment = await GetAppointmentWithNavigationAsync(appointmentId, ct);

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed or AppointmentStatus.NoShow)
            throw new BusinessRuleViolationException("CannotReschedule",
                $"Cannot reschedule an appointment with status '{appointment.Status}'.");

        if (request.NewAppointmentTime <= DateTime.UtcNow)
            throw new BusinessRuleViolationException("PastAppointment", "New appointment time must be in the future.");

        var endTime = request.NewAppointmentTime.AddMinutes(appointment.Doctor.AverageConsultationMinutes);

        // Check new slot availability using Schedule Engine
        var isAvailable = await _scheduleService.IsDoctorAvailableAsync(appointment.DoctorId, request.NewAppointmentTime, endTime, ct);
        if (!isAvailable)
            throw new BusinessRuleViolationException("SlotUnavailable", "The new time slot is not available.");

        appointment.AppointmentTime = request.NewAppointmentTime;
        appointment.EndTime = request.NewAppointmentTime.AddMinutes(appointment.Doctor.AverageConsultationMinutes);
        appointment.Status = AppointmentStatus.Scheduled;

        _uow.Appointments.Update(appointment);
        await _uow.CompleteAsync(ct);


        return await MapToResponseDtoAsync(appointment, appointment.Patient, appointment.Doctor);
    }

    /// <inheritdoc/>
    public async Task ConfirmAppointmentAsync(Guid id, CancellationToken ct = default)
    {
        var appt = await _uow.Appointments.GetByIdAsync(id, ct) ?? throw new NotFoundException("Appointment", id);

        if (appt.Status != AppointmentStatus.Scheduled)
            throw new BusinessRuleViolationException("InvalidState", "Only Scheduled appointments can be confirmed.");

        appt.Status = AppointmentStatus.Confirmed;
        _uow.Appointments.Update(appt);
        await _uow.CompleteAsync(ct);

        _logger.LogInformation("Appointment {AppointmentId} confirmed", id);
    }

    /// <inheritdoc/>
    public async Task MarkNoShowAsync(Guid appointmentId, CancellationToken ct = default)
    {
        var appointment = await GetAppointmentWithNavigationAsync(appointmentId, ct);

        if (appointment.Status != AppointmentStatus.Scheduled && appointment.Status != AppointmentStatus.Confirmed)
            throw new BusinessRuleViolationException("InvalidStatus",
                "Only Scheduled/Confirmed appointments can be marked as no-show.");

        var minutesPast = (DateTime.UtcNow - appointment.AppointmentTime).TotalMinutes;
        if (minutesPast < AppConstants.Appointment.NoShowMinutes)
            throw new BusinessRuleViolationException("TooEarlyForNoShow",
                $"No-show can only be marked after {AppConstants.Appointment.NoShowMinutes} minutes past appointment time.");

        appointment.Status = AppointmentStatus.NoShow;

        // Increment patient no-show count
        var patient = appointment.Patient;
        patient.NoShowCount++;
        if (patient.NoShowCount >= 3) patient.IsPriority = false; // lose priority after 3 no-shows
        _uow.Patients.Update(patient);

        _uow.Appointments.Update(appointment);
        await _uow.CompleteAsync(ct);

        _logger.LogWarning("No-show recorded for appointment {Id}, Patient {PatientId}", appointmentId, patient.Id);
    }

    /// <inheritdoc/>
    public async Task<AppointmentResponseDto> CheckInPatientAsync(Guid appointmentId, CancellationToken ct = default)
    {
        var appointment = await GetAppointmentWithNavigationAsync(appointmentId, ct);

        if (appointment.Status != AppointmentStatus.Scheduled && appointment.Status != AppointmentStatus.Confirmed)
            throw new BusinessRuleViolationException("InvalidStatus", "Patient can only check-in for Scheduled/Confirmed appointments.");

        appointment.Status = AppointmentStatus.CheckedIn;
        appointment.CheckedInAt = DateTime.UtcNow;

        _uow.Appointments.Update(appointment);
        await _uow.CompleteAsync(ct);

        return await MapToResponseDtoAsync(appointment, appointment.Patient, appointment.Doctor);
    }

    /// <inheritdoc/>
    public async Task<AppointmentResponseDto> StartAppointmentAsync(Guid appointmentId, CancellationToken ct = default)
    {
        var appointment = await GetAppointmentWithNavigationAsync(appointmentId, ct);

        if (appointment.Status != AppointmentStatus.CheckedIn)
            throw new BusinessRuleViolationException("InvalidStatus", "Appointment must be in CheckedIn status to start.");

        appointment.Status = AppointmentStatus.InProgress;
        _uow.Appointments.Update(appointment);
        await _uow.CompleteAsync(ct);

        return await MapToResponseDtoAsync(appointment, appointment.Patient, appointment.Doctor);
    }

    /// <inheritdoc/>
    public async Task<AppointmentResponseDto> CompleteAppointmentAsync(Guid appointmentId, CancellationToken ct = default)
    {
        var appointment = await GetAppointmentWithNavigationAsync(appointmentId, ct);

        if (appointment.Status != AppointmentStatus.InProgress)
            throw new BusinessRuleViolationException("InvalidStatus", "Only InProgress appointments can be completed.");

        appointment.Status = AppointmentStatus.Completed;
        appointment.CompletedAt = DateTime.UtcNow;

        _uow.Appointments.Update(appointment);
        await _uow.CompleteAsync(ct);

        // Auto-generate billing
        await _billing.GenerateBillForAppointmentAsync(appointmentId, ct);

        _logger.LogInformation("Appointment {Id} completed. Billing auto-generated.", appointmentId);

        return await MapToResponseDtoAsync(appointment, appointment.Patient, appointment.Doctor);
    }

    /// <inheritdoc/>
    public async Task<PagedResult<AppointmentResponseDto>> GetPatientAppointmentsAsync(
        Guid patientUserId, PaginationFilter filter, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.FirstOrDefaultAsync(p => p.UserId == patientUserId, ct)
            ?? throw new NotFoundException("Patient profile not found.");

        var query = _uow.Appointments.Query()
            .Where(a => a.PatientId == patient.Id)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(a => a.Reason.Contains(filter.SearchTerm) || a.Doctor.FirstName.Contains(filter.SearchTerm));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.AppointmentTime)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var dtos = items.Select(a => MapToResponseDto(a)).ToList();
        return PagedResult<AppointmentResponseDto>.Create(dtos, total, filter.PageNumber, filter.PageSize);
    }

    /// <inheritdoc/>
    public async Task<PagedResult<AppointmentResponseDto>> GetDoctorAppointmentsAsync(
        Guid doctorUserId, PaginationFilter filter, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId, ct)
            ?? throw new NotFoundException("Doctor profile not found.");

        var query = _uow.Appointments.Query()
            .Where(a => a.DoctorId == doctor.Id)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(a => a.Patient.FirstName.Contains(filter.SearchTerm)
                                  || a.Patient.LastName.Contains(filter.SearchTerm));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(a => a.AppointmentTime)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var dtos = items.Select(a => MapToResponseDto(a)).ToList();
        return PagedResult<AppointmentResponseDto>.Create(dtos, total, filter.PageNumber, filter.PageSize);
    }

    /// <inheritdoc/>
    public async Task<AppointmentResponseDto> GetByIdAsync(Guid appointmentId, CancellationToken ct = default)
    {
        var appointment = await GetAppointmentWithNavigationAsync(appointmentId, ct);
        return MapToResponseDto(appointment);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<Appointment> GetAppointmentWithNavigationAsync(Guid id, CancellationToken ct)
    {
        var appointment = await _uow.Appointments.Query()
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException("Appointment", id);

        return appointment;
    }

    private static Task<AppointmentResponseDto> MapToResponseDtoAsync(Appointment a, Patient p, Doctor d)
        => Task.FromResult(MapToResponseDto(a, p, d));

    private static AppointmentResponseDto MapToResponseDto(Appointment a, Patient? patient = null, Doctor? doctor = null)
    {
        var p = patient ?? a.Patient;
        var d = doctor ?? a.Doctor;

        List<string>? symptoms = null;
        if (!string.IsNullOrEmpty(a.SymptomsJson))
        {
            try { symptoms = JsonSerializer.Deserialize<List<string>>(a.SymptomsJson); }
            catch { /* ignore malformed JSON */ }
        }

        return new AppointmentResponseDto
        {
            Id = a.Id,
            PatientId = p.Id,
            PatientName = $"{p.FirstName} {p.LastName}",
            DoctorId = d.Id,
            DoctorName = $"Dr. {d.FirstName} {d.LastName}",
            DoctorSpecialization = d.Specialization,
            AppointmentTime = a.AppointmentTime,
            EndTime = a.EndTime,
            Status = a.Status.ToString(),
            Type = a.Type.ToString(),
            Reason = a.Reason,
            Symptoms = symptoms,
            Priority = a.Priority.ToString(),
            CancellationReason = a.CancellationReason,
            ReminderSent = a.ReminderSent,
            LateCancellationPenalty = a.LateCancellationPenalty,
            CheckedInAt = a.CheckedInAt,
            CompletedAt = a.CompletedAt,
            CreatedAt = a.CreatedAt
        };
    }
}
