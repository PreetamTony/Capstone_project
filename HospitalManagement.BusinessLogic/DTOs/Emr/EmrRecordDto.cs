namespace HospitalManagement.BusinessLogic.DTOs.Emr;

public class EmrRecordDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string? FamilyHistory { get; set; }
    public string? SocialHistory { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Nested DTOs
    public List<AllergyDto> Allergies { get; set; } = new();
    public List<MedicalHistoryDto> MedicalHistories { get; set; } = new();
    public List<VitalsDto> Vitals { get; set; } = new();
}
