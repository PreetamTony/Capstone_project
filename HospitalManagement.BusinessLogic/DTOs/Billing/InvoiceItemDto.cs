namespace HospitalManagement.BusinessLogic.DTOs.Billing;

public class InvoiceItemDto
{
    public Guid Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
}
