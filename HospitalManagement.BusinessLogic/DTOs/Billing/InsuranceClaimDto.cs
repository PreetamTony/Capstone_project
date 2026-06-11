namespace HospitalManagement.BusinessLogic.DTOs.Billing;

public class InsuranceClaimDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string InsuranceProvider { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? RejectionReason { get; set; }
}
