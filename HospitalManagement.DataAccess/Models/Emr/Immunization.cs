namespace HospitalManagement.DataAccess.Models.Emr;

public class Immunization : BaseEntity
{
    public Guid EmrRecordId { get; set; }
    
    public string VaccineName { get; set; } = string.Empty;
    public DateTime DateAdministered { get; set; }
    public string DoseNumber { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? Notes { get; set; }

    // Navigation
    public EmrRecord EmrRecord { get; set; } = null!;
}
