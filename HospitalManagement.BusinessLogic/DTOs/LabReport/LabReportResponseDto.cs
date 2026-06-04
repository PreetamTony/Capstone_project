namespace HospitalManagement.BusinessLogic.DTOs.LabReport;

public class LabReportResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid? VisitId { get; set; }
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string ReportName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string? Observations { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsConfidential { get; set; }
    public string? OriginalFileName { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}
