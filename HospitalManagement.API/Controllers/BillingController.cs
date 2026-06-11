using HospitalManagement.BusinessLogic.DTOs.Billing;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Route("api/billing")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    // Get Invoices

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Receptionist,Patient")]
    public async Task<IActionResult> GetInvoiceById(Guid id, CancellationToken ct)
    {
        var result = await _billingService.GetInvoiceByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("visit/{visitId}")]
    [Authorize(Roles = "Admin,Receptionist,Patient")]
    public async Task<IActionResult> GetInvoiceByVisitId(Guid visitId, CancellationToken ct)
    {
        var result = await _billingService.GetInvoiceByVisitIdAsync(visitId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("patient/{patientId}")]
    [Authorize(Roles = "Admin,Receptionist,Patient")]
    public async Task<IActionResult> GetInvoicesByPatient(Guid patientId, CancellationToken ct)
    {
        // TODO: Ensure patientId matches current user if role is Patient
        var result = await _billingService.GetInvoicesByPatientAsync(patientId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("patient/me")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetMyInvoices(CancellationToken ct)
    {
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();
        
        var result = await _billingService.GetInvoicesByUserIdAsync(userId, ct);
        return Ok(new { success = true, data = result });
    }

    // Invoice Generation & PDF

    [HttpPost("generate/visit/{visitId}")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> GenerateInvoiceForVisit(Guid visitId, CancellationToken ct)
    {
        var result = await _billingService.GenerateInvoiceForVisitAsync(visitId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}/invoice")]
    [Authorize(Roles = "Admin,Receptionist,Patient")]
    public async Task<IActionResult> GetInvoicePdf(Guid id, CancellationToken ct)
    {
        var pdfBytes = await _billingService.GetInvoicePdfAsync(id, ct);
        return File(pdfBytes, "application/pdf", $"Invoice_{id}.pdf");
    }

    // Payments
    [HttpGet("{id}/payments")]
    [Authorize(Roles = "Admin,Receptionist,Patient")]
    public async Task<IActionResult> GetPayments(Guid id, CancellationToken ct)
    {
        var result = await _billingService.GetInvoicePaymentsAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id}/create-payment-intent")]
    [Authorize(Roles = "Admin,Receptionist,Patient")]
    public async Task<IActionResult> CreateStripePaymentIntent(Guid id, CancellationToken ct)
    {
        var clientSecret = await _billingService.CreateStripePaymentIntentAsync(id, ct);
        return Ok(new { success = true, clientSecret });
    }

    [HttpPost("{id}/confirm-stripe-payment")]
    [Authorize(Roles = "Admin,Receptionist,Patient")]
    public async Task<IActionResult> ConfirmStripePayment(Guid id, [FromBody] ConfirmStripePaymentDto request, CancellationToken ct)
    {
        var result = await _billingService.ConfirmStripePaymentAsync(id, request.PaymentIntentId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id}/payments")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> ProcessPayment(Guid id, [FromBody] ProcessPaymentDto request, CancellationToken ct)
    {
        var result = await _billingService.ProcessPaymentAsync(id, request, ct);
        return Ok(new { success = true, data = result });
    }

    // Refunds

    [HttpPost("{id}/refund")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ProcessRefund(Guid id, [FromBody] ProcessRefundDto request, CancellationToken ct)
    {
        var result = await _billingService.ProcessRefundAsync(id, request, ct);
        return Ok(new { success = true, data = result });
    }

    // Insurance Claims

    [HttpGet("{id}/insurance-claim")]
    [Authorize(Roles = "Admin,Receptionist,Patient")]
    public async Task<IActionResult> GetInsuranceClaims(Guid id, CancellationToken ct)
    {
        var result = await _billingService.GetInvoiceInsuranceClaimsAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id}/insurance-claim")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> ProcessInsuranceClaim(Guid id, [FromBody] ProcessInsuranceClaimDto request, CancellationToken ct)
    {
        var result = await _billingService.ProcessInsuranceClaimAsync(id, request, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPut("{id}/insurance-claim/{claimId}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveInsuranceClaim(Guid id, Guid claimId, [FromBody] ProcessInsuranceClaimApprovalDto request, CancellationToken ct)
    {
        var result = await _billingService.ApproveInsuranceClaimAsync(id, claimId, request, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPut("{id}/insurance-claim/{claimId}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectInsuranceClaim(Guid id, Guid claimId, [FromBody] RejectInsuranceClaimDto request, CancellationToken ct)
    {
        var result = await _billingService.RejectInsuranceClaimAsync(id, claimId, request, ct);
        return Ok(new { success = true, data = result });
    }

    // Cancel

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CancelInvoice(Guid id, [FromBody] CancelInvoiceRequest request, CancellationToken ct)
    {
        await _billingService.CancelInvoiceAsync(id, request.Reason, ct);
        return Ok(new { success = true, message = "Invoice cancelled successfully" });
    }

    // Analytics

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetBillingStatistics(CancellationToken ct)
    {
        var result = await _billingService.GetBillingStatisticsAsync(ct);
        return Ok(new { success = true, data = result });
    }
}

public class CancelInvoiceRequest
{
    public string Reason { get; set; } = string.Empty;
}
