using HospitalManagement.BusinessLogic.DTOs.Schedule;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IScheduleService
{
    Task<List<TimeSlotDto>> GetAvailableSlotsAsync(Guid doctorId, DateTime date, CancellationToken ct = default);
    Task<bool> IsDoctorAvailableAsync(Guid doctorId, DateTime startDateTime, DateTime endDateTime, CancellationToken ct = default);
    Task<List<DoctorScheduleDto>> GetDoctorScheduleAsync(Guid doctorId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task ApplyLeaveAsync(Guid doctorId, ApplyLeaveRequestDto request, CancellationToken ct = default);
    Task BlockSlotAsync(Guid doctorId, BlockSlotRequestDto request, CancellationToken ct = default);
}
