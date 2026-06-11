using HospitalManagement.BusinessLogic.DTOs.Appointment;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public class SlotEngine : ISlotEngine
{
    private readonly IUnitOfWork _uow;

    public SlotEngine(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<AvailableSlotDto> GetAvailableSlotsAsync(Guid doctorId, DateTime date, CancellationToken ct = default)
    {
        var targetDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var dayOfWeek = targetDate.DayOfWeek;

        // 1. Get Doctor's schedule for this day
        var schedule = await _uow.DoctorSchedules.Query()
            .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == (int)dayOfWeek, ct);

        if (schedule == null)
            return new AvailableSlotDto { DoctorId = doctorId, SlotDurationMinutes = 15, AvailableSlots = new() };

        // 2. Check if Doctor is on leave
        var isOnLeave = await _uow.DoctorLeaves.Query()
            .AnyAsync(l => l.DoctorId == doctorId 
                           && l.IsApproved
                           && l.StartDateTime.Date <= targetDate 
                           && l.EndDateTime.Date >= targetDate, ct);

        if (isOnLeave)
            return new AvailableSlotDto { DoctorId = doctorId, SlotDurationMinutes = 15, AvailableSlots = new() };

        // 3. Get existing appointments for the day (that are not cancelled/no-show)
        var existingAppointments = await _uow.Appointments.Query()
            .Where(a => a.DoctorId == doctorId 
                        && a.AppointmentTime.Date == targetDate 
                        && a.Status != AppointmentStatus.Cancelled 
                        && a.Status != AppointmentStatus.NoShow)
            .Select(a => a.AppointmentTime)
            .ToListAsync(ct);

        // 4. Get blocked slots
        var blockedSlots = await _uow.BlockedSlots.Query()
            .Where(b => b.DoctorId == doctorId && b.StartDateTime.Date == targetDate)
            .ToListAsync(ct);

        // Fetch Doctor to get AverageConsultationMinutes
        var doctor = await _uow.Doctors.GetByIdAsync(doctorId, ct);
        var averageMins = doctor?.AverageConsultationMinutes ?? 15;

        // 5. Generate slots
        var availableSlots = new List<DateTime>();
        var currentTime = targetDate.Add(schedule.StartTime);
        var endTime = targetDate.Add(schedule.EndTime);
        var slotDuration = TimeSpan.FromMinutes(averageMins);

        while (currentTime + slotDuration <= endTime)
        {
            bool isBooked = existingAppointments.Contains(currentTime);
            bool isBlocked = blockedSlots.Any(b => currentTime >= b.StartDateTime && currentTime < b.EndDateTime);
            
            // Only add slots that are in the future if it's today
            bool isFuture = currentTime > DateTime.UtcNow;

            if (!isBooked && !isBlocked && isFuture)
            {
                availableSlots.Add(currentTime);
            }

            currentTime = currentTime.Add(slotDuration);
        }

        return new AvailableSlotDto
        {
            DoctorId = doctorId,
            SlotDurationMinutes = averageMins,
            AvailableSlots = availableSlots
        };
    }
}
