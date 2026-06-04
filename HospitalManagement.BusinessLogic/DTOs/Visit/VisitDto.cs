namespace HospitalManagement.BusinessLogic.DTOs.Visit;

public class VisitDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public Guid? AppointmentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public DateTime? DischargeTime { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? Diagnosis { get; set; }
    public string? ClinicalNotes { get; set; }
}
