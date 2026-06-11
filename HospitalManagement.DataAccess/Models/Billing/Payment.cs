using HospitalManagement.DataAccess.Models.Enums.Billing;

namespace HospitalManagement.DataAccess.Models.Billing;

public class Payment : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public Invoice Invoice { get; set; } = null!;
}
