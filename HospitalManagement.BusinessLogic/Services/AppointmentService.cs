using System.Text.Json;
using HospitalManagement.BusinessLogic.DTOs.Appointment;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.DataAccess.Constants;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Repositories;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.BusinessLogic.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _uow;
    private readonly IBillingService _billing;
    private readonly ISlotEngine _slotEngine;
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IUnitOfWork uow,
        IBillingService billing, 
        ISlotEngine slotEngine, 
        INotificationService notificationService,
        ICurrentUserService currentUserService,
        ILogger<AppointmentService> logger)
    {
        _uow = uow;
        _billing = billing;
        _slotEngine = slotEngine;
        _notificationService = notificationService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<AvailableSlotDto> GetAvailableSlotsAsync(Guid doctorId, DateTime date, CancellationToken ct = default)
    {
        return await _slotEngine.GetAvailableSlotsAsync(doctorId, date, ct);
    }

    public async Task<AppointmentDetailsDto> BookAppointmentAsync(BookAppointmentRequestDto request, CancellationToken ct = default)
    {
        Patient patient;
        if (_currentUserService.Role == HospitalManagement.DataAccess.Constants.AppConstants.Roles.Patient)
        {
            patient = await _uow.Patients.FirstOrDefaultAsync(p => p.UserId == _currentUserService.UserId.Value, ct)
                ?? throw new NotFoundException("Patient profile not found for this user.");
            request.PatientId = patient.Id;
        }
        else
        {
            patient = await _uow.Patients.GetByIdAsync(request.PatientId, ct)
                ?? throw new NotFoundException("Patient profile not found.");
        }

        var doctor = await _uow.Doctors.GetByIdAsync(request.DoctorId, ct)
            ?? throw new NotFoundException("Doctor", request.DoctorId);


        if (request.AppointmentTime <= DateTime.UtcNow)
            throw new BusinessRuleViolationException("PastAppointment", "Cannot book an appointment in the past.");

        // Check availability via Slot Engine
        var availableSlotsDto = await _slotEngine.GetAvailableSlotsAsync(doctor.Id, request.AppointmentTime.Date, ct);
        if (!availableSlotsDto.AvailableSlots.Contains(request.AppointmentTime))
            throw new BusinessRuleViolationException("SlotUnavailable", "The selected time slot is not available. It may be blocked, outside schedule, or the doctor is on leave.");

        // Daily patient limit
        var todayCount = await _uow.Appointments.CountAsync(
            a => a.DoctorId == request.DoctorId
              && a.AppointmentTime.Date == request.AppointmentTime.Date
              && a.Status != AppointmentStatus.Cancelled
              && a.Status != AppointmentStatus.NoShow, ct);

        if (todayCount >= doctor.MaxPatientsPerDay)
            throw new BusinessRuleViolationException("DailyLimitReached", $"Doctor has reached the maximum of {doctor.MaxPatientsPerDay} patients for this day.");

        // Duplicate Check
        var isDuplicate = await _uow.Appointments.AnyAsync(
            a => a.PatientId == patient.Id 
              && a.AppointmentTime == request.AppointmentTime
              && a.Status != AppointmentStatus.Cancelled, ct);
              
        if (isDuplicate)
            throw new BusinessRuleViolationException("DuplicateBooking", "You already have an appointment at this exact time.");

        var appointment = new Appointment
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            AppointmentTime = request.AppointmentTime,
            EndTime = request.AppointmentTime.AddMinutes(availableSlotsDto.SlotDurationMinutes),
            Status = AppointmentStatus.Scheduled,
            Type = request.Type,
            Reason = request.Reason,
            SymptomsJson = request.Symptoms != null ? JsonSerializer.Serialize(request.Symptoms) : null,
            Priority = request.Priority,
            
            Source = DetermineSource(_currentUserService.Role),
            CreatedByUserId = _currentUserService.UserId,
            BookedByRole = _currentUserService.Role
        };

        await _uow.Appointments.AddAsync(appointment, ct);

        if (appointment.Type == AppointmentType.Video)
        {
            appointment.IsTeleConsultation = true;
            appointment.MeetingProvider = "WebRTC";
            appointment.MeetingUrl = $"/video-consultation/{appointment.Id}";
        }

        await _uow.CompleteAsync(ct);

        _logger.LogInformation("Appointment {Id} booked for Patient {PatientId} with Doctor {DoctorId} at {Time} by {Role}",
            appointment.Id, patient.Id, doctor.Id, appointment.AppointmentTime, _currentUserService.Role);

        // Events
        await _notificationService.NotifyAppointmentBookedAsync(patient.Id, appointment.Id, ct);

        return await MapToDetailsDtoAsync(appointment, patient, doctor);
    }

    public async Task<AppointmentDetailsDto> CancelAppointmentAsync(Guid appointmentId, CancelAppointmentRequestDto request, CancellationToken ct = default)
    {
        var appointment = await GetAppointmentWithNavigationAsync(appointmentId, ct);

        AppointmentStateMachine.ValidateTransition(appointment.Status, AppointmentStatus.Cancelled);

        var hoursUntilAppointment = (appointment.AppointmentTime - DateTime.UtcNow).TotalHours;
        var latePenalty = hoursUntilAppointment < AppConstants.Appointment.CancellationNoticeHours;

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = request.Reason;
        appointment.CancelledBy = _currentUserService.UserId;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.LateCancellationPenalty = latePenalty;

        _uow.Appointments.Update(appointment);
        await _uow.CompleteAsync(ct);

        return await MapToDetailsDtoAsync(appointment, appointment.Patient, appointment.Doctor);
    }

    public async Task<AppointmentDetailsDto> RescheduleAppointmentAsync(Guid appointmentId, RescheduleAppointmentRequestDto request, CancellationToken ct = default)
    {
        var appointment = await GetAppointmentWithNavigationAsync(appointmentId, ct);

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed or AppointmentStatus.NoShow)
            throw new BusinessRuleViolationException("CannotReschedule", $"Cannot reschedule an appointment with status '{appointment.Status}'.");

        if (request.NewAppointmentTime <= DateTime.UtcNow)
            throw new BusinessRuleViolationException("PastAppointment", "New appointment time must be in the future.");

        var availableSlotsDto = await _slotEngine.GetAvailableSlotsAsync(appointment.DoctorId, request.NewAppointmentTime.Date, ct);
        if (!availableSlotsDto.AvailableSlots.Contains(request.NewAppointmentTime))
            throw new BusinessRuleViolationException("SlotUnavailable", "The new time slot is not available.");

        appointment.AppointmentTime = request.NewAppointmentTime;
        appointment.EndTime = request.NewAppointmentTime.AddMinutes(availableSlotsDto.SlotDurationMinutes);
        
        appointment.RescheduledByUserId = _currentUserService.UserId;
        appointment.RescheduledAt = DateTime.UtcNow;

        _uow.Appointments.Update(appointment);
        await _uow.CompleteAsync(ct);

        return await MapToDetailsDtoAsync(appointment, appointment.Patient, appointment.Doctor);
    }

    public async Task<AppointmentDetailsDto> ConfirmAppointmentAsync(Guid id, CancellationToken ct = default)
    {
        var appt = await GetAppointmentWithNavigationAsync(id, ct);
        
        AppointmentStateMachine.ValidateTransition(appt.Status, AppointmentStatus.Confirmed);

        appt.Status = AppointmentStatus.Confirmed;
        appt.ConfirmationSentAt = DateTime.UtcNow;
        
        _uow.Appointments.Update(appt);
        await _uow.CompleteAsync(ct);

        _logger.LogInformation("Appointment {AppointmentId} confirmed", id);
        return await MapToDetailsDtoAsync(appt, appt.Patient, appt.Doctor);
    }

    public async Task<AppointmentDetailsDto> MarkNoShowAsync(Guid appointmentId, CancellationToken ct = default)
    {
        var appointment = await GetAppointmentWithNavigationAsync(appointmentId, ct);

        AppointmentStateMachine.ValidateTransition(appointment.Status, AppointmentStatus.NoShow);

        var minutesPast = (DateTime.UtcNow - appointment.AppointmentTime).TotalMinutes;
        if (minutesPast < AppConstants.Appointment.NoShowMinutes)
            throw new BusinessRuleViolationException("TooEarlyForNoShow", $"No-show can only be marked after {AppConstants.Appointment.NoShowMinutes} minutes past appointment time.");

        appointment.Status = AppointmentStatus.NoShow;

        var patient = appointment.Patient;
        patient.NoShowCount++;
        if (patient.NoShowCount >= 3) patient.IsPriority = false;
        
        _uow.Patients.Update(patient);
        _uow.Appointments.Update(appointment);
        await _uow.CompleteAsync(ct);

        return await MapToDetailsDtoAsync(appointment, appointment.Patient, appointment.Doctor);
    }

    public async Task<AppointmentDetailsDto> CheckInPatientAsync(Guid appointmentId, CancellationToken ct = default)
    {
        var appointment = await GetAppointmentWithNavigationAsync(appointmentId, ct);

        AppointmentStateMachine.ValidateTransition(appointment.Status, AppointmentStatus.CheckedIn);

        appointment.Status = AppointmentStatus.CheckedIn;
        appointment.CheckedInAt = DateTime.UtcNow;
        appointment.CheckInByUserId = _currentUserService.UserId;

        // Optionally assign queue number here
        // appointment.QueueNumber = GenerateQueueNumber();

        _uow.Appointments.Update(appointment);
        await _uow.CompleteAsync(ct);

        return await MapToDetailsDtoAsync(appointment, appointment.Patient, appointment.Doctor);
    }

    public async Task<AppointmentDetailsDto> StartAppointmentAsync(Guid appointmentId, CancellationToken ct = default)
    {
        var appointment = await GetAppointmentWithNavigationAsync(appointmentId, ct);

        AppointmentStateMachine.ValidateTransition(appointment.Status, AppointmentStatus.InProgress);

        appointment.Status = AppointmentStatus.InProgress;
        
        // Phase 11: Visit Integration
        var visit = new Visit
        {
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            AppointmentId = appointment.Id,
            CheckInTime = DateTime.UtcNow,
            Status = VisitStatus.InConsultation,
            ChiefComplaint = appointment.Reason
        };
        await _uow.Visits.AddAsync(visit, ct);

        _uow.Appointments.Update(appointment);
        await _uow.CompleteAsync(ct);
        
        appointment.Visit = visit;

        return await MapToDetailsDtoAsync(appointment, appointment.Patient, appointment.Doctor);
    }

    public async Task<AppointmentDetailsDto> CompleteAppointmentAsync(Guid appointmentId, CancellationToken ct = default)
    {
        var appointment = await GetAppointmentWithNavigationAsync(appointmentId, ct);

        AppointmentStateMachine.ValidateTransition(appointment.Status, AppointmentStatus.Completed);

        appointment.Status = AppointmentStatus.Completed;
        appointment.CompletedAt = DateTime.UtcNow;
        
        // Update associated visit
        if (appointment.Visit == null)
        {
            appointment.Visit = await _uow.Visits.Query().FirstOrDefaultAsync(v => v.AppointmentId == appointment.Id, ct);
        }
        
        if (appointment.Visit != null)
        {
            appointment.Visit.Status = VisitStatus.Completed;
            appointment.Visit.DischargeTime = DateTime.UtcNow;
            _uow.Visits.Update(appointment.Visit);
        }

        _uow.Appointments.Update(appointment);
        await _uow.CompleteAsync(ct);

        // Phase 10: Auto-generate billing (This will likely mark it PendingPayment)
        if (appointment.Visit != null)
        {
            await _billing.GenerateInvoiceForVisitAsync(appointment.Visit.Id, ct);
        }

        _logger.LogInformation("Appointment {Id} completed. Billing auto-generated.", appointmentId);

        return await MapToDetailsDtoAsync(appointment, appointment.Patient, appointment.Doctor);
    }

    public async Task<PagedResult<AppointmentSummaryDto>> GetAppointmentsAsync(AppointmentFilterDto filter, CancellationToken ct = default)
    {
        var query = _uow.Appointments.Query()
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .AsQueryable();

        if (filter.DoctorId.HasValue)
            query = query.Where(a => a.DoctorId == filter.DoctorId.Value);
            
        if (filter.PatientId.HasValue)
            query = query.Where(a => a.PatientId == filter.PatientId.Value);

        if (filter.Status.HasValue)
            query = query.Where(a => a.Status == filter.Status.Value);
            
        if (filter.Type.HasValue)
            query = query.Where(a => a.Type == filter.Type.Value);
            
        if (filter.Priority.HasValue)
            query = query.Where(a => a.Priority == filter.Priority.Value);
            
        if (filter.FromDate.HasValue)
            query = query.Where(a => a.AppointmentTime >= filter.FromDate.Value);
            
        if (filter.ToDate.HasValue)
            query = query.Where(a => a.AppointmentTime <= filter.ToDate.Value);

        // Sorting
        query = filter.Sorting switch
        {
            "date_asc" => query.OrderBy(a => a.AppointmentTime),
            "date_desc" => query.OrderByDescending(a => a.AppointmentTime),
            _ => query.OrderByDescending(a => a.AppointmentTime)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var dtos = items.Select(MapToSummaryDto).ToList();
        return PagedResult<AppointmentSummaryDto>.Create(dtos, total, filter.PageNumber, filter.PageSize);
    }

    public async Task<AppointmentDetailsDto> GetByIdAsync(Guid appointmentId, CancellationToken ct = default)
    {
        var appointment = await GetAppointmentWithNavigationAsync(appointmentId, ct);
        return MapToDetailsDto(appointment, appointment.Patient, appointment.Doctor);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<Appointment> GetAppointmentWithNavigationAsync(Guid id, CancellationToken ct)
    {
        var appointment = await _uow.Appointments.Query()
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Visit)
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException("Appointment", id);

        var userRole = _currentUserService.Role;
        var userId = _currentUserService.UserId;

        if (userRole == HospitalManagement.DataAccess.Constants.AppConstants.Roles.Patient && appointment.Patient.UserId != userId)
        {
            throw new HospitalManagement.DataAccess.Exceptions.BusinessRuleViolationException("Forbidden", "You are not authorized to access this appointment.");
        }
        
        if (userRole == HospitalManagement.DataAccess.Constants.AppConstants.Roles.Doctor && appointment.Doctor.UserId != userId)
        {
            throw new HospitalManagement.DataAccess.Exceptions.BusinessRuleViolationException("Forbidden", "You are not authorized to access this appointment.");
        }

        return appointment;
    }

    private static AppointmentSource DetermineSource(string? role)
    {
        return role switch
        {
            AppConstants.Roles.Patient => AppointmentSource.PatientPortal,
            AppConstants.Roles.Receptionist => AppointmentSource.Receptionist,
            AppConstants.Roles.Doctor => AppointmentSource.Doctor,
            AppConstants.Roles.Admin => AppointmentSource.Admin,
            _ => AppointmentSource.PatientPortal
        };
    }

    private static AppointmentSummaryDto MapToSummaryDto(Appointment a)
    {
        return new AppointmentSummaryDto
        {
            Id = a.Id,
            DoctorName = $"Dr. {a.Doctor.FirstName} {a.Doctor.LastName}",
            DoctorSpecialization = a.Doctor.Specialization,
            PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
            AppointmentTime = a.AppointmentTime,
            Status = a.Status.ToString(),
            Type = a.Type.ToString(),
            Priority = a.Priority.ToString()
        };
    }

    private static Task<AppointmentDetailsDto> MapToDetailsDtoAsync(Appointment a, Patient p, Doctor d)
        => Task.FromResult(MapToDetailsDto(a, p, d));

    private static AppointmentDetailsDto MapToDetailsDto(Appointment a, Patient p, Doctor d)
    {
        var summary = MapToSummaryDto(a);
        
        List<string>? symptoms = null;
        if (!string.IsNullOrEmpty(a.SymptomsJson))
        {
            try { symptoms = JsonSerializer.Deserialize<List<string>>(a.SymptomsJson); }
            catch { /* ignore malformed JSON */ }
        }

        return new AppointmentDetailsDto
        {
            Id = summary.Id,
            DoctorName = summary.DoctorName,
            DoctorSpecialization = summary.DoctorSpecialization,
            PatientName = summary.PatientName,
            AppointmentTime = summary.AppointmentTime,
            Status = summary.Status,
            Type = summary.Type,
            Priority = summary.Priority,
            
            Reason = a.Reason,
            SymptomsJson = a.SymptomsJson,
            Notes = a.Notes,
            QueueNumber = a.QueueNumber,
            ConsultationRoom = a.ConsultationRoom,
            IsTeleConsultation = a.IsTeleConsultation,
            MeetingUrl = a.MeetingUrl,
            MeetingProvider = a.MeetingProvider,
            
            CheckedInAt = a.CheckedInAt,
            CompletedAt = a.CompletedAt,
            CancellationReason = a.CancellationReason
        };
    }
}
