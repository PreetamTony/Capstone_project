namespace HospitalManagement.DataAccess.Models.Emr;

public class MedicalHistory : BaseEntity
{
    public Guid EmrRecordId { get; set; }
    
    public string Condition { get; set; } = string.Empty;
    public DateTime? DiagnosisDate { get; set; }
    public string Status { get; set; } = "Active"; // e.g., Active, Resolved
    public string? Notes { get; set; }

    // Navigation
    public EmrRecord EmrRecord { get; set; } = null!;
}
