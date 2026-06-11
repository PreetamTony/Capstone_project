using HospitalManagement.DataAccess.Models.Enums.Billing;

namespace HospitalManagement.DataAccess.Models.Billing;

public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public InvoiceItemType ItemType { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }

    public Invoice Invoice { get; set; } = null!;
}
