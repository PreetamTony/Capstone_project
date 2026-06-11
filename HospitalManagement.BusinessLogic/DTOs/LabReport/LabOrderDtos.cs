using Microsoft.AspNetCore.Http;

namespace HospitalManagement.BusinessLogic.DTOs.LabReport;

public class CreateLabOrderRequestDto
{
    public Guid ConsultationId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string Priority { get; set; } = "Routine";
    public string? Notes { get; set; }
    public bool IsConfidential { get; set; } = false;
}

public class UpdateLabReportStatusDto
{
    public string Status { get; set; } = string.Empty; // e.g. "SampleCollected", "InProgress", "Completed"
}

public class ReviewLabReportDto
{
    public string Notes { get; set; } = string.Empty;
}

public class LabReportStatisticsDto
{
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int CompletedReports { get; set; }
    public int ReviewedReports { get; set; }
}

public class LabReportResponseDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid? ConsultationId { get; set; }
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    
    public string TestName { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? OrderNotes { get; set; }
    public string? ReviewNotes { get; set; }
    
    public string ReportName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string? Observations { get; set; }
    
    public string Status { get; set; } = string.Empty;
    public bool IsConfidential { get; set; }
    public string? OriginalFileName { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UploadLabReportRequestDto
{
    public string ReportName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string? Observations { get; set; }
    public IFormFile File { get; set; } = null!;
}
