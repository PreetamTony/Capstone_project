using HospitalManagement.BusinessLogic.DTOs.Appointment;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface ISlotEngine
{
    Task<AvailableSlotDto> GetAvailableSlotsAsync(Guid doctorId, DateTime date, CancellationToken ct = default);
}
