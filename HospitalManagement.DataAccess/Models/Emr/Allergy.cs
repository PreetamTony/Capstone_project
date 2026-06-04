namespace HospitalManagement.DataAccess.Models.Emr;

public class Allergy : BaseEntity
{
    public Guid EmrRecordId { get; set; }
    
    public string Substance { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // e.g., Mild, Moderate, Severe
    public string Reaction { get; set; } = string.Empty; // e.g., Hives, Anaphylaxis
    public string? Notes { get; set; }

    // Navigation
    public EmrRecord EmrRecord { get; set; } = null!;
}
