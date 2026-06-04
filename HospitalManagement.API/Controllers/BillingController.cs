using System.Security.Claims;
using HospitalManagement.BusinessLogic.DTOs.Billing;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.Services;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

/// <summary>Billing — outstanding bills, payment processing, and invoice retrieval.</summary>
[ApiController]
[Route("api/billing")]
[Authorize]
[Produces("application/json")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;
    private readonly ILogger<BillingController> _logger;

    public BillingController(IBillingService billingService, ILogger<BillingController> logger)
    {
        _billingService = billingService;
        _logger = logger;
    }

    /// <summary>Get a bill by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BillingResponseDto), 200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _billingService.GetByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get billing record for a specific visit.</summary>
    [HttpGet("visit/{visitId:guid}")]
    [ProducesResponseType(typeof(BillingResponseDto), 200)]
    public async Task<IActionResult> GetByVisit(Guid visitId, CancellationToken ct)
    {
        var result = await _billingService.GetByAppointmentIdAsync(visitId, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Generate bill for a specific visit.</summary>
    [HttpPost("generate/visit/{visitId}")]
    [Authorize(Roles = $"{AppConstants.Roles.Receptionist},{AppConstants.Roles.Admin}")]
    [ProducesResponseType(typeof(BillingResponseDto), 200)]
    public async Task<IActionResult> GenerateBill(Guid visitId, CancellationToken ct)
    {
        var result = await _billingService.GenerateBillForAppointmentAsync(visitId, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get all outstanding bills for the current patient.</summary>
    [HttpGet("outstanding")]
    [Authorize(Roles = AppConstants.Roles.Patient)]
    [ProducesResponseType(typeof(PagedResult<BillingResponseDto>), 200)]
    public async Task<IActionResult> GetOutstanding([FromQuery] PaginationFilter filter, CancellationToken ct)
    {
        var result = await _billingService.GetPatientOutstandingBillsAsync(GetCurrentUserId(), filter, ct);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Pay a bill (Patient only).</summary>
    [HttpPost("{id:guid}/pay")]
    [Authorize(Roles = AppConstants.Roles.Patient)]
    [ProducesResponseType(typeof(BillingResponseDto), 200)]
    public async Task<IActionResult> Pay(Guid id, [FromBody] PaymentRequestDto request, CancellationToken ct)
    {
        var result = await _billingService.ProcessPaymentAsync(id, GetCurrentUserId(), request, ct);
        return Ok(new { success = true, data = result });
    }

    private Guid GetCurrentUserId()
    {
        var id = User.FindFirstValue(AppConstants.Jwt.ClaimUserId)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID claim not found.");
        return Guid.Parse(id);
    }
}
