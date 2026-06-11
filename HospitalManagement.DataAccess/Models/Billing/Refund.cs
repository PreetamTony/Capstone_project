namespace HospitalManagement.DataAccess.Models.Billing;

public class Refund : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;

    public Invoice Invoice { get; set; } = null!;
    public Payment Payment { get; set; } = null!;
}
