namespace HospitalManagement.BusinessLogic.DTOs.Patient;

public class PatientTimelineItemDto
{
    public DateTime Date { get; set; }
    public string EventType { get; set; } = string.Empty; // "Appointment", "Visit", "Prescription", "LabReport", "Bill"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
}
