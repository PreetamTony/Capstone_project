namespace HospitalManagement.BusinessLogic.DTOs.Queue;

public class QueueEntryDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public Guid VisitId { get; set; }
    public int TokenNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CheckedInAt { get; set; }
    public DateTime? CalledAt { get; set; }
    public DateTime? ConsultationStartedAt { get; set; }
    public DateTime? ConsultationEndedAt { get; set; }
}
