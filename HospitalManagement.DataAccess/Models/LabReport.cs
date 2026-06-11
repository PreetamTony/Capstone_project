using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.DataAccess.Models;

/// <summary>
/// Lab report uploaded by a lab technician or doctor, linked to a patient.
/// </summary>
public class LabReport : BaseEntity
{
    public Guid PatientId { get; set; }
    public Guid? ConsultationId { get; set; }
    public Guid DoctorId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string Priority { get; set; } = "Routine";
    public string? OrderNotes { get; set; }
    public string? ReviewNotes { get; set; }
    
    public string ReportName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Observations { get; set; }
    public LabReportStatus Status { get; set; } = LabReportStatus.Ordered;
    public bool IsConfidential { get; set; } = false;
    public Guid UploadedBy { get; set; }
    public string? OriginalFileName { get; set; }
    public long FileSizeBytes { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation
    public Patient Patient { get; set; } = null!;
    public Consultation? Consultation { get; set; }
    public Doctor Doctor { get; set; } = null!;
}
