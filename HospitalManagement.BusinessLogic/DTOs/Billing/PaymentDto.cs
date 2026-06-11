namespace HospitalManagement.BusinessLogic.DTOs.Billing;

public class PaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
