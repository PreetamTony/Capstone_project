namespace HospitalManagement.BusinessLogic.DTOs.Billing;

public class ProcessInsuranceClaimDto
{
    public string InsuranceProvider { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public decimal ClaimAmount { get; set; }
    public string? Notes { get; set; }
}
