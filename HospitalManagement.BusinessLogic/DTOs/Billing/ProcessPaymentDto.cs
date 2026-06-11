namespace HospitalManagement.BusinessLogic.DTOs.Billing;

public class ProcessPaymentDto
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Cash, CreditCard, DebitCard, Insurance, BankTransfer
    public string? TransactionId { get; set; }
    public string? Notes { get; set; }
}
