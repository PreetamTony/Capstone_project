using HospitalManagement.BusinessLogic.DTOs.Visit;

namespace HospitalManagement.BusinessLogic.DTOs.Emr;

public class FullEmrResponseDto
{
    public Guid EmrRecordId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public List<AllergyDto> Allergies { get; set; } = new();
    public List<MedicalHistoryDto> MedicalHistory { get; set; } = new();
    
    public List<VisitSummaryDto> PastVisits { get; set; } = new();
}

public class VisitSummaryDto
{
    public Guid VisitId { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? DischargeTime { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string ChiefComplaint { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string TreatmentPlan { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    
    // Aggregated clinical data for this visit
    public VitalsDto? Vitals { get; set; }
    public List<string> PrescribedMedications { get; set; } = new();
    public List<string> LabReports { get; set; } = new();
}
