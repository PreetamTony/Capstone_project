namespace HospitalManagement.BusinessLogic.DTOs.Emr;

public class MedicalHistoryDto
{
    public Guid Id { get; set; }
    public string Condition { get; set; } = string.Empty;
    public DateTime? DiagnosisDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
