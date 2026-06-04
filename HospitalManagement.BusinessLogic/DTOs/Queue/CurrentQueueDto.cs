namespace HospitalManagement.BusinessLogic.DTOs.Queue;

public class CurrentQueueDto
{
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public QueueEntryDto? CurrentPatient { get; set; }
    public List<QueueEntryDto> WaitingPatients { get; set; } = new();
    public int TotalWaiting { get; set; }
}
