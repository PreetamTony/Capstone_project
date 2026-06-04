namespace HospitalManagement.BusinessLogic.DTOs.Appointment;

public class CancelAppointmentRequestDto
{
    public string Reason { get; set; } = string.Empty;
}

public class RescheduleAppointmentRequestDto
{
    public DateTime NewAppointmentTime { get; set; }
    public string? Reason { get; set; }
}
