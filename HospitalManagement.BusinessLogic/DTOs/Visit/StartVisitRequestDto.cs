namespace HospitalManagement.BusinessLogic.DTOs.Visit;

public class StartVisitRequestDto
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid? AppointmentId { get; set; }
    public string? ChiefComplaint { get; set; }
}
