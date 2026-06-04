using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.DataAccess.Models;

/// <summary>
/// Lab report uploaded by a lab technician or doctor, linked to a patient.
/// </summary>
public class LabReport : BaseEntity
{
    public Guid PatientId { get; set; }
    public Guid? VisitId { get; set; }
    public Guid DoctorId { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Observations { get; set; }
    public LabReportStatus Status { get; set; } = LabReportStatus.Pending;
    public bool IsConfidential { get; set; } = false;
    public Guid UploadedBy { get; set; }
    public string? OriginalFileName { get; set; }
    public long FileSizeBytes { get; set; }

    // Navigation
    public Patient Patient { get; set; } = null!;
    public Visit? Visit { get; set; }
    public Doctor Doctor { get; set; } = null!;
}
