using HospitalManagement.BusinessLogic.DTOs.Appointment;
using HospitalManagement.BusinessLogic.DTOs.Common;

namespace HospitalManagement.BusinessLogic.Services;

/// <summary>Appointment lifecycle management service contract.</summary>
public interface IAppointmentService
{
    Task<AppointmentResponseDto> BookAppointmentAsync(Guid patientUserId, BookAppointmentRequestDto request, CancellationToken ct = default);
    Task<List<AvailableSlotResponseDto>> GetAvailableSlotsAsync(Guid doctorId, DateTime date, CancellationToken ct = default);
    Task<AppointmentResponseDto> CancelAppointmentAsync(Guid appointmentId, Guid cancelledByUserId, CancelAppointmentRequestDto request, CancellationToken ct = default);
    Task<AppointmentResponseDto> RescheduleAppointmentAsync(Guid appointmentId, Guid userId, RescheduleAppointmentRequestDto request, CancellationToken ct = default);
    Task ConfirmAppointmentAsync(Guid appointmentId, CancellationToken ct = default);
    Task MarkNoShowAsync(Guid appointmentId, CancellationToken ct = default);
    Task<AppointmentResponseDto> CheckInPatientAsync(Guid appointmentId, CancellationToken ct = default);
    Task<AppointmentResponseDto> StartAppointmentAsync(Guid appointmentId, CancellationToken ct = default);
    Task<AppointmentResponseDto> CompleteAppointmentAsync(Guid appointmentId, CancellationToken ct = default);
    Task<PagedResult<AppointmentResponseDto>> GetPatientAppointmentsAsync(Guid patientUserId, PaginationFilter filter, CancellationToken ct = default);
    Task<PagedResult<AppointmentResponseDto>> GetDoctorAppointmentsAsync(Guid doctorUserId, PaginationFilter filter, CancellationToken ct = default);
    Task<AppointmentResponseDto> GetByIdAsync(Guid appointmentId, CancellationToken ct = default);
}
