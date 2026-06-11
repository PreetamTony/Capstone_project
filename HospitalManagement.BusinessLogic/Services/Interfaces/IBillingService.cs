using HospitalManagement.BusinessLogic.DTOs.Billing;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IBillingService
{
    Task<InvoiceDto> GenerateInvoiceForVisitAsync(Guid visitId, CancellationToken ct = default);
    Task<string> CreateStripePaymentIntentAsync(Guid invoiceId, CancellationToken ct = default);
    Task<InvoiceDto> ConfirmStripePaymentAsync(Guid invoiceId, string paymentIntentId, CancellationToken ct = default);
    Task<InvoiceDto> ProcessPaymentAsync(Guid invoiceId, ProcessPaymentDto request, CancellationToken ct = default);
    Task<InvoiceDto> ProcessInsuranceClaimAsync(Guid invoiceId, ProcessInsuranceClaimDto request, CancellationToken ct = default);
    Task<InvoiceDto> ProcessRefundAsync(Guid paymentId, ProcessRefundDto request, CancellationToken ct = default);
    Task<InvoiceDto> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken ct = default);
    Task<InvoiceDto> GetInvoiceByVisitIdAsync(Guid visitId, CancellationToken ct = default);
    Task<List<InvoiceDto>> GetInvoicesByPatientAsync(Guid patientId, CancellationToken ct = default);
    Task<List<InvoiceDto>> GetInvoicesByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task CancelInvoiceAsync(Guid invoiceId, string reason, CancellationToken ct = default);
    
    // Payments & Insurance 
    Task<List<PaymentDto>> GetInvoicePaymentsAsync(Guid invoiceId, CancellationToken ct = default);
    Task<List<InsuranceClaimDto>> GetInvoiceInsuranceClaimsAsync(Guid invoiceId, CancellationToken ct = default);
    Task<InvoiceDto> ApproveInsuranceClaimAsync(Guid invoiceId, Guid claimId, ProcessInsuranceClaimApprovalDto request, CancellationToken ct = default);
    Task<InvoiceDto> RejectInsuranceClaimAsync(Guid invoiceId, Guid claimId, RejectInsuranceClaimDto request, CancellationToken ct = default);

    // Analytics & PDF
    Task<BillingStatisticsDto> GetBillingStatisticsAsync(CancellationToken ct = default);
    Task<byte[]> GetInvoicePdfAsync(Guid invoiceId, CancellationToken ct = default);
}
