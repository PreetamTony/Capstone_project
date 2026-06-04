using System.Security.Claims;
using HospitalManagement.BusinessLogic.DTOs.Queue;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QueueController : ControllerBase
{
    private readonly IQueueService _queueService;

    public QueueController(IQueueService queueService)
    {
        _queueService = queueService;
    }

    [HttpGet("doctor/{doctorId}")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Receptionist}")]
    public async Task<ActionResult<CurrentQueueDto>> GetCurrentQueue(Guid doctorId, CancellationToken ct)
    {
        var result = await _queueService.GetCurrentQueueAsync(doctorId, ct);
        return Ok(result);
    }

    [HttpPut("doctor/{doctorId}/call-next")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<ActionResult<QueueEntryDto>> CallNext(Guid doctorId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _queueService.CallNextAsync(doctorId, userId, ct);
        return Ok(result);
    }

    [HttpPut("doctor/{doctorId}/skip/{tokenNumber}")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<ActionResult<QueueEntryDto>> SkipToken(Guid doctorId, int tokenNumber, CancellationToken ct)
    {
        var result = await _queueService.SkipTokenAsync(doctorId, tokenNumber, ct);
        return Ok(result);
    }

    [HttpPut("doctor/{doctorId}/recall/{tokenNumber}")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<ActionResult<QueueEntryDto>> RecallPatient(Guid doctorId, int tokenNumber, CancellationToken ct)
    {
        var result = await _queueService.RecallPatientAsync(doctorId, tokenNumber, ct);
        return Ok(result);
    }

    [HttpPut("{queueEntryId}/no-show")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor},{AppConstants.Roles.Receptionist}")]
    public async Task<ActionResult<QueueEntryDto>> MarkNoShow(Guid queueEntryId, CancellationToken ct)
    {
        var result = await _queueService.MarkNoShowAsync(queueEntryId, ct);
        return Ok(result);
    }
}
