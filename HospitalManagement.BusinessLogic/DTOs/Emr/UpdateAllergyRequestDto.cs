namespace HospitalManagement.BusinessLogic.DTOs.Emr;

public class UpdateAllergyRequestDto
{
    public string Substance { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Reaction { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
}
