using System.Security.Claims;
using HospitalManagement.BusinessLogic.DTOs.Queue;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QueueController : ControllerBase
{
    private readonly IQueueService _queueService;
    private readonly IUnitOfWork _uow;

    public QueueController(IQueueService queueService, IUnitOfWork uow)
    {
        _queueService = queueService;
        _uow = uow;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<Guid> GetDoctorIdAsync()
    {
        var userId = GetUserId();
        var doctor = await _uow.Doctors.Query().FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) throw new UnauthorizedAccessException("Only doctors can access this endpoint.");
        return doctor.Id;
    }

    [HttpGet("doctor/me")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<ActionResult<CurrentQueueDto>> GetCurrentQueue(CancellationToken ct)
    {
        var doctorId = await GetDoctorIdAsync();
        var result = await _queueService.GetCurrentQueueAsync(doctorId, ct);
        return Ok(result);
    }

    [HttpPut("doctor/me/call-next")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<ActionResult<QueueEntryDto>> CallNext(CancellationToken ct)
    {
        var doctorId = await GetDoctorIdAsync();
        var result = await _queueService.CallNextAsync(doctorId, ct);
        return Ok(result);
    }

    [HttpPut("doctor/me/skip/{tokenNumber}")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<ActionResult<QueueEntryDto>> SkipToken(int tokenNumber, CancellationToken ct)
    {
        var doctorId = await GetDoctorIdAsync();
        var result = await _queueService.SkipTokenAsync(doctorId, tokenNumber, ct);
        return Ok(result);
    }

    [HttpPut("doctor/me/recall/{tokenNumber}")]
    [Authorize(Roles = AppConstants.Roles.Doctor)]
    public async Task<ActionResult<QueueEntryDto>> RecallPatient(int tokenNumber, CancellationToken ct)
    {
        var doctorId = await GetDoctorIdAsync();
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

    [HttpGet("my-position")]
    [Authorize(Roles = AppConstants.Roles.Patient)]
    public async Task<ActionResult<QueuePositionDto>> GetMyPosition(CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _queueService.GetMyPositionAsync(userId, ct);
        return Ok(result);
    }

    [HttpPut("{queueEntryId}/rejoin")]
    [Authorize(Roles = $"{AppConstants.Roles.Receptionist},{AppConstants.Roles.Admin}")]
    public async Task<ActionResult<QueueEntryDto>> RejoinQueue(Guid queueEntryId, CancellationToken ct)
    {
        var result = await _queueService.RejoinQueueAsync(queueEntryId, ct);
        return Ok(result);
    }

    [HttpPut("{queueEntryId}/complete")]
    [Authorize(Roles = $"{AppConstants.Roles.Doctor}")]
    public async Task<ActionResult<QueueEntryDto>> CompleteQueueEntry(Guid queueEntryId, CancellationToken ct)
    {
        var result = await _queueService.CompleteQueueEntryAsync(queueEntryId, ct);
        return Ok(result);
    }

    [HttpGet("display/{doctorId}")]
    [AllowAnonymous]
    public async Task<ActionResult<QueueDisplayDto>> GetDisplayQueue(Guid doctorId, CancellationToken ct)
    {
        var result = await _queueService.GetDisplayQueueAsync(doctorId, ct);
        return Ok(result);
    }

    [HttpGet("statistics")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<ActionResult<QueueStatisticsDto>> GetStatistics([FromQuery] DateTime? date, CancellationToken ct)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var result = await _queueService.GetQueueStatisticsAsync(targetDate, ct);
        return Ok(result);
    }
}
