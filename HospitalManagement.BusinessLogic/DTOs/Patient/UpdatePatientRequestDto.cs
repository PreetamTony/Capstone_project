using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.BusinessLogic.DTOs.Patient;

public class UpdatePatientRequestDto
{
    public string? Address { get; set; }
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public decimal? InsuranceCoveragePercent { get; set; }
    public BloodGroup? BloodGroup { get; set; }
}
