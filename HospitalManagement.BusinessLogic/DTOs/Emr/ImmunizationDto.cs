namespace HospitalManagement.BusinessLogic.DTOs.Emr;

public class ImmunizationDto
{
    public Guid Id { get; set; }
    public string VaccineName { get; set; } = string.Empty;
    public DateTime DateAdministered { get; set; }
    public string DoseNumber { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class CreateImmunizationRequestDto
{
    public string VaccineName { get; set; } = string.Empty;
    public DateTime DateAdministered { get; set; }
    public string DoseNumber { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateImmunizationRequestDto
{
    public string VaccineName { get; set; } = string.Empty;
    public DateTime DateAdministered { get; set; }
    public string DoseNumber { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
