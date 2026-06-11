using HospitalManagement.BusinessLogic.DTOs.Schedule;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IScheduleService
{
    Task<List<DoctorScheduleDto>> CreateDoctorScheduleAsync(Guid doctorId, CreateScheduleRequestDto request, CancellationToken ct = default);
    Task UpdateDoctorScheduleAsync(Guid doctorId, Guid scheduleId, UpdateScheduleRequestDto request, CancellationToken ct = default);
    Task DeleteDoctorScheduleAsync(Guid doctorId, Guid scheduleId, CancellationToken ct = default);
    Task<List<TimeSlotDto>> GetAvailableSlotsAsync(Guid doctorId, DateTime date, CancellationToken ct = default);
    Task<bool> IsDoctorAvailableAsync(Guid doctorId, DateTime startDateTime, DateTime endDateTime, CancellationToken ct = default);
    Task<List<DoctorScheduleDto>> GetDoctorScheduleAsync(Guid doctorId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task ApplyLeaveAsync(Guid doctorId, ApplyLeaveRequestDto request, CancellationToken ct = default);
    Task BlockSlotAsync(Guid doctorId, BlockSlotRequestDto request, CancellationToken ct = default);
}
