namespace HospitalManagement.DataAccess.Models;

public class AdmissionRecord : BaseEntity
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid BedId { get; set; }
    public DateTime AdmissionDate { get; set; }
    public DateTime? DischargeDate { get; set; }
    public string Status { get; set; } = "Admitted"; // "Admitted", "Discharged", "Transferred"
    public string? DischargeSummary { get; set; }

    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Bed Bed { get; set; } = null!;
}
