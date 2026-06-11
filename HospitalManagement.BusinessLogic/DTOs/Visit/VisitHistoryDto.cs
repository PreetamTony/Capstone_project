namespace HospitalManagement.BusinessLogic.DTOs.Visit;

public class VisitHistoryDto
{
    public Guid Id { get; set; }
    public string PreviousState { get; set; } = string.Empty;
    public string NewState { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}
