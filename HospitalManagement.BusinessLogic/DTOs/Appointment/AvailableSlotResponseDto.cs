namespace HospitalManagement.BusinessLogic.DTOs.Appointment;

public class AvailableSlotResponseDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
}
