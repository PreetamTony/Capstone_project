namespace HospitalManagement.BusinessLogic.DTOs.Billing;

public class BillingResponseDto
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal InsuranceCoverage { get; set; }
    public decimal PatientResponsibility { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? TransactionId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentRequestDto
{
    public string PaymentMethod { get; set; } = string.Empty;    // Cash, Card, Insurance, UPI
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
}
