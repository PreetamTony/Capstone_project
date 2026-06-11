using HospitalManagement.BusinessLogic.DTOs.Appointment;
using HospitalManagement.BusinessLogic.DTOs.Common;

namespace HospitalManagement.BusinessLogic.Services;

/// <summary>Appointment lifecycle management service contract.</summary>
public interface IAppointmentService
{
    Task<AvailableSlotDto> GetAvailableSlotsAsync(Guid doctorId, DateTime date, CancellationToken ct = default);
    Task<AppointmentDetailsDto> BookAppointmentAsync(BookAppointmentRequestDto request, CancellationToken ct = default);
    Task<AppointmentDetailsDto> CancelAppointmentAsync(Guid appointmentId, CancelAppointmentRequestDto request, CancellationToken ct = default);
    Task<AppointmentDetailsDto> RescheduleAppointmentAsync(Guid appointmentId, RescheduleAppointmentRequestDto request, CancellationToken ct = default);
    
    Task<AppointmentDetailsDto> ConfirmAppointmentAsync(Guid appointmentId, CancellationToken ct = default);
    Task<AppointmentDetailsDto> MarkNoShowAsync(Guid appointmentId, CancellationToken ct = default);
    Task<AppointmentDetailsDto> CheckInPatientAsync(Guid appointmentId, CancellationToken ct = default);
    Task<AppointmentDetailsDto> StartAppointmentAsync(Guid appointmentId, CancellationToken ct = default);
    Task<AppointmentDetailsDto> CompleteAppointmentAsync(Guid appointmentId, CancellationToken ct = default);
    
    Task<PagedResult<AppointmentSummaryDto>> GetAppointmentsAsync(AppointmentFilterDto filter, CancellationToken ct = default);
    Task<AppointmentDetailsDto> GetByIdAsync(Guid appointmentId, CancellationToken ct = default);
}
