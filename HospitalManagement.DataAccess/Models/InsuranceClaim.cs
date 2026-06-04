namespace HospitalManagement.DataAccess.Models;

public class InsuranceClaim : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid VisitId { get; set; }
    public Visit Visit { get; set; } = null!;

    public string Provider { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public decimal ClaimAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
}
