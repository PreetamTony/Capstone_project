using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.BusinessLogic.DTOs.Appointment;

public class BookAppointmentRequestDto
{
    public Guid DoctorId { get; set; }
    public DateTime AppointmentTime { get; set; }
    public AppointmentType Type { get; set; } = AppointmentType.InPerson;
    public string Reason { get; set; } = string.Empty;
    public List<string>? Symptoms { get; set; }
    public AppointmentPriority Priority { get; set; } = AppointmentPriority.Medium;
    public Guid PatientId { get; set; }
}
