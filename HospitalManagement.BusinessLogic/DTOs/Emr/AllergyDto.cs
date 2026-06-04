namespace HospitalManagement.BusinessLogic.DTOs.Emr;

public class AllergyDto
{
    public Guid Id { get; set; }
    public string Substance { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Reaction { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
