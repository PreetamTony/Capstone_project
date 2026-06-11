using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Models.Enums.Billing;

namespace HospitalManagement.DataAccess.Models.Billing;

public class InsuranceClaim : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public string ClaimNumber { get; set; } = string.Empty;
    public string InsuranceProvider { get; set; } = string.Empty;
    
    public decimal RequestedAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    
    public InsuranceClaimStatus Status { get; set; } = InsuranceClaimStatus.Pending;
    
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    
    public string? RejectionReason { get; set; }
}
