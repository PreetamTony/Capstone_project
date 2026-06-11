namespace HospitalManagement.BusinessLogic.DTOs.Visit;

public class VisitSummaryDto
{
    public Guid Id { get; set; }
    public string VisitNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string VisitType { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
}
