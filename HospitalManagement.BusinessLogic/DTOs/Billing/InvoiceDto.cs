namespace HospitalManagement.BusinessLogic.DTOs.Billing;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid VisitId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal InsuranceCoverage { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    
    public string Status { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }

    public List<InvoiceItemDto> Items { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
}
