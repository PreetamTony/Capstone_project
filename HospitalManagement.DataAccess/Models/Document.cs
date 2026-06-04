namespace HospitalManagement.DataAccess.Models;

public class Document : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public string DocumentType { get; set; } = string.Empty; // e.g., MRI, CTScan, XRay, DischargeSummary
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}
