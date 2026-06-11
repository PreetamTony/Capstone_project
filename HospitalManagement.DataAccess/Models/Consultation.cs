namespace HospitalManagement.DataAccess.Models;

public class Consultation : BaseEntity
{
    public Guid VisitId { get; set; }
    public Visit? Visit { get; set; }
    
    public Guid DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public string ChiefComplaint { get; set; } = string.Empty;
    public List<string> Symptoms { get; set; } = new();
    public string Assessment { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string DiagnosisCode { get; set; } = string.Empty;
    public string TreatmentPlan { get; set; } = string.Empty;
    
    public string Notes { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public string FollowUpInstructions { get; set; } = string.Empty;
    public DateTime? FollowUpDate { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public HospitalManagement.DataAccess.Models.Enums.ConsultationStatus Status { get; set; } = HospitalManagement.DataAccess.Models.Enums.ConsultationStatus.Draft;
    
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<LabReport> LabReports { get; set; } = new List<LabReport>();
}
