using HospitalManagement.BusinessLogic.DTOs.Schedule;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace HospitalManagement.BusinessLogic.Services;

public class ScheduleService : IScheduleService
{
    private readonly IUnitOfWork _uow;
    private readonly IDistributedCache _cache;

    public ScheduleService(IUnitOfWork uow, IDistributedCache cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<List<DoctorScheduleDto>> CreateDoctorScheduleAsync(Guid doctorId, CreateScheduleRequestDto request, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(doctorId, ct) 
            ?? throw new NotFoundException("Doctor", doctorId);

        var schedules = new List<DoctorSchedule>();

        foreach (var day in request.DaysOfWeek)
        {
            var schedule = new DoctorSchedule
            {
                DoctorId = doctorId,
                DayOfWeek = day,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsRecurring = request.IsRecurring,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo
            };

            await _uow.DoctorSchedules.AddAsync(schedule, ct);
            schedules.Add(schedule);
        }

        await _uow.CompleteAsync(ct);

        return schedules.Select(s => new DoctorScheduleDto
        {
            Id = s.Id,
            DoctorId = s.DoctorId,
            DayOfWeek = s.DayOfWeek,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            IsRecurring = s.IsRecurring,
            ValidFrom = s.ValidFrom,
            ValidTo = s.ValidTo
        }).ToList();
    }

    public async Task UpdateDoctorScheduleAsync(Guid doctorId, Guid scheduleId, UpdateScheduleRequestDto request, CancellationToken ct = default)
    {
        var schedule = await _uow.DoctorSchedules.GetByIdAsync(scheduleId, ct)
            ?? throw new NotFoundException("DoctorSchedule", scheduleId);

        if (schedule.DoctorId != doctorId)
            throw new BusinessRuleViolationException("InvalidDoctor", "Schedule does not belong to the specified doctor.");

        schedule.DayOfWeek = request.DayOfWeek;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;
        schedule.IsRecurring = request.IsRecurring;
        schedule.ValidFrom = request.ValidFrom;
        schedule.ValidTo = request.ValidTo;

        _uow.DoctorSchedules.Update(schedule);
        await _uow.CompleteAsync(ct);
    }

    public async Task DeleteDoctorScheduleAsync(Guid doctorId, Guid scheduleId, CancellationToken ct = default)
    {
        var schedule = await _uow.DoctorSchedules.GetByIdAsync(scheduleId, ct)
            ?? throw new NotFoundException("DoctorSchedule", scheduleId);

        if (schedule.DoctorId != doctorId)
            throw new BusinessRuleViolationException("InvalidDoctor", "Schedule does not belong to the specified doctor.");

        schedule.IsDeleted = true;
        
        _uow.DoctorSchedules.Update(schedule);
        await _uow.CompleteAsync(ct);
    }

    public async Task<List<TimeSlotDto>> GetAvailableSlotsAsync(Guid doctorId, DateTime date, CancellationToken ct = default)
    {
        var cacheKey = $"AvailableSlots_{doctorId}_{date:yyyyMMdd}";
        var cachedData = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<List<TimeSlotDto>>(cachedData) ?? new List<TimeSlotDto>();
        }

        var targetDate = date.Date;
        var dayOfWeek = (int)targetDate.DayOfWeek;
        // Adjust standard DayOfWeek to our enum (1=Mon, 7=Sun) if needed, but let's assume .NET DayOfWeek (0=Sun, 6=Sat) is what we use, wait, requirement says Monday=1, Sunday=7.
        var targetDay = dayOfWeek == 0 ? 7 : dayOfWeek;

        // 1. Get Base Schedule
        var schedules = await _uow.DoctorSchedules.Query()
            .Where(s => s.DoctorId == doctorId && s.DayOfWeek == targetDay 
                   && s.ValidFrom.Date <= targetDate && s.ValidTo.Date >= targetDate)
            .ToListAsync(ct);

        if (!schedules.Any())
            return new List<TimeSlotDto>(); // No schedule for this day

        // 2. Check Leaves
        var leaves = await _uow.DoctorLeaves.Query()
            .Where(l => l.DoctorId == doctorId && l.IsApproved 
                   && l.StartDateTime.Date <= targetDate && l.EndDateTime.Date >= targetDate)
            .ToListAsync(ct);

        if (leaves.Any(l => l.StartDateTime <= targetDate && l.EndDateTime >= targetDate.AddDays(1)))
            return new List<TimeSlotDto>(); // Doctor is on leave the whole day

        // 3. Blocked Slots & Existing Appointments
        var blockedSlots = await _uow.BlockedSlots.Query()
            .Where(b => b.DoctorId == doctorId && b.StartDateTime.Date == targetDate)
            .ToListAsync(ct);

        var appointments = await _uow.Appointments.Query()
            .Where(a => a.DoctorId == doctorId && a.AppointmentTime.Date == targetDate 
                   && a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.NoShow)
            .ToListAsync(ct);

        var availableSlots = new List<TimeSlotDto>();
        var slotDuration = TimeSpan.FromMinutes(30);

        foreach (var sched in schedules)
        {
            var currentTime = sched.StartTime;
            while (currentTime.Add(slotDuration) <= sched.EndTime)
            {
                var slotEnd = currentTime.Add(slotDuration);
                var slotStartDt = targetDate.Add(currentTime);
                var slotEndDt = targetDate.Add(slotEnd);

                bool isBlocked = blockedSlots.Any(b => b.StartDateTime < slotEndDt && b.EndDateTime > slotStartDt);
                bool hasLeave = leaves.Any(l => l.StartDateTime < slotEndDt && l.EndDateTime > slotStartDt);
                bool isBooked = appointments.Any(a => a.AppointmentTime >= slotStartDt && a.AppointmentTime < slotEndDt);

                availableSlots.Add(new TimeSlotDto
                {
                    StartTime = currentTime,
                    EndTime = slotEnd,
                    IsAvailable = !isBlocked && !hasLeave && !isBooked && slotStartDt > DateTime.UtcNow
                });

                currentTime = slotEnd;
            }
        }

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(availableSlots), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // Cache for 5 mins as slots change frequently
        }, ct);

        return availableSlots;
    }

    public async Task<bool> IsDoctorAvailableAsync(Guid doctorId, DateTime startDateTime, DateTime endDateTime, CancellationToken ct = default)
    {
        var availableSlots = await GetAvailableSlotsAsync(doctorId, startDateTime.Date, ct);
        return availableSlots.Any(s => s.StartTime <= startDateTime.TimeOfDay && s.EndTime >= endDateTime.TimeOfDay && s.IsAvailable);
    }

    public async Task<List<DoctorScheduleDto>> GetDoctorScheduleAsync(Guid doctorId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var schedules = await _uow.DoctorSchedules.Query()
            .Where(s => s.DoctorId == doctorId && s.ValidFrom <= endDate && s.ValidTo >= startDate)
            .ToListAsync(ct);

        return schedules.Select(s => new DoctorScheduleDto
        {
            Id = s.Id,
            DoctorId = s.DoctorId,
            DayOfWeek = s.DayOfWeek,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            IsRecurring = s.IsRecurring,
            ValidFrom = s.ValidFrom,
            ValidTo = s.ValidTo
        }).ToList();
    }

    public async Task ApplyLeaveAsync(Guid doctorId, ApplyLeaveRequestDto request, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(doctorId, ct)
            ?? throw new NotFoundException("Doctor", doctorId);

        // Validation against appointments is handled separately, but we could add it here
        var appointments = await _uow.Appointments.Query()
            .Where(a => a.DoctorId == doctorId && a.AppointmentTime >= request.StartDateTime && a.AppointmentTime <= request.EndDateTime
                   && a.Status != AppointmentStatus.Cancelled)
            .AnyAsync(ct);

        if (appointments)
            throw new BusinessRuleViolationException("HasAppointments", "Cannot apply leave. Doctor has existing appointments in this timeframe.");

        var leave = new DoctorLeave
        {
            DoctorId = doctorId,
            LeaveType = request.LeaveType,
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
            Reason = request.Reason,
            IsApproved = true // Auto-approve for demo
        };

        await _uow.DoctorLeaves.AddAsync(leave, ct);
        await _uow.CompleteAsync(ct);
    }

    public async Task BlockSlotAsync(Guid doctorId, BlockSlotRequestDto request, CancellationToken ct = default)
    {
        var block = new BlockedSlot
        {
            DoctorId = doctorId,
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
            BlockReason = request.BlockReason,
            Description = request.Description
        };

        await _uow.BlockedSlots.AddAsync(block, ct);
        await _uow.CompleteAsync(ct);
    }
}
