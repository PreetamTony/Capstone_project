namespace HospitalManagement.BusinessLogic.DTOs.Emr;

public class EmrSummaryDto
{
    public Guid PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? BloodGroup { get; set; }
    
    public int ActiveAllergiesCount { get; set; }
    public int ActiveConditionsCount { get; set; }
    
    public VitalsDto? LatestVitals { get; set; }
    
    public DateTime? LastVisitDate { get; set; }
    public DateTime? LastConsultationDate { get; set; }
    
    public int TotalVisits { get; set; }
    public int TotalConsultations { get; set; }
}
