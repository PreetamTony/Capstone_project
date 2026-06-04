using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.DataAccess.Models;

/// <summary>
/// Billing record auto-generated when a visit is completed.
/// PatientResponsibility = Amount - InsuranceCoverage (computed in service layer).
/// </summary>
public class Billing : BaseEntity
{
    public Guid VisitId { get; set; }
    public Guid PatientId { get; set; }
    public decimal Amount { get; set; }
    public decimal InsuranceCoverage { get; set; } = 0;
    public decimal PatientResponsibility { get; set; }
    public BillingCategory Category { get; set; } = BillingCategory.Consultation;
    public BillingStatus Status { get; set; } = BillingStatus.Pending;
    public string? PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? TransactionId { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Visit Visit { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}
