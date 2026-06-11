namespace HospitalManagement.BusinessLogic.DTOs.Queue;

public class QueuePositionDto
{
    public int TokenNumber { get; set; }
    public int Position { get; set; }
    public int EstimatedWaitMinutes { get; set; }
    public string DoctorName { get; set; } = string.Empty;
}
