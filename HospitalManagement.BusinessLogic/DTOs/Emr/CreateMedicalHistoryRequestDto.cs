namespace HospitalManagement.BusinessLogic.DTOs.Emr;

public class CreateMedicalHistoryRequestDto
{
    public string Condition { get; set; } = string.Empty;
    public DateTime? DiagnosisDate { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
}
