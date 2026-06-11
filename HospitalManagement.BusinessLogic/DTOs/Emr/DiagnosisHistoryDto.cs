namespace HospitalManagement.BusinessLogic.DTOs.Emr;

public class DiagnosisHistoryDto
{
    public string Diagnosis { get; set; } = string.Empty;
    public string? IcdCode { get; set; }
    public DateTime? DiagnosedOn { get; set; }
    public string DiagnosedBy { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
}
