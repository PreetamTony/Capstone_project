namespace HospitalManagement.DataAccess.Models.Emr;

public class EmrRecord : BaseEntity
{
    public Guid PatientId { get; set; }
    
    // Blood type might be here if not strictly in Patient, but we have it in Patient.
    // EMR specific details:
    public string? FamilyHistory { get; set; }
    public string? SocialHistory { get; set; }
    
    // Navigation
    public Patient Patient { get; set; } = null!;
    
    public ICollection<Allergy> Allergies { get; set; } = new List<Allergy>();
    public ICollection<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();
    public ICollection<Vitals> Vitals { get; set; } = new List<Vitals>();
}
