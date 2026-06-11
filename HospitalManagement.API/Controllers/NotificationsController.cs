using System.Security.Claims;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationDto>), 200)]
    public async Task<IActionResult> GetMyNotifications(CancellationToken ct)
    {
        var result = await _notificationService.GetUserNotificationsAsync(GetCurrentUserId(), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("unread")]
    [ProducesResponseType(typeof(List<NotificationDto>), 200)]
    public async Task<IActionResult> GetUnreadNotifications(CancellationToken ct)
    {
        var result = await _notificationService.GetUnreadNotificationsAsync(GetCurrentUserId(), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var count = await _notificationService.GetUnreadCountAsync(GetCurrentUserId(), ct);
        return Ok(new { success = true, count });
    }

    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        await _notificationService.MarkAsReadAsync(id, GetCurrentUserId(), ct);
        return NoContent();
    }

    [HttpPut("read-all")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        await _notificationService.MarkAllAsReadAsync(GetCurrentUserId(), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteNotification(Guid id, CancellationToken ct)
    {
        await _notificationService.DeleteNotificationAsync(id, GetCurrentUserId(), ct);
        return NoContent();
    }

    [HttpPost]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequestDto request, CancellationToken ct)
    {
        await _notificationService.CreateNotificationAsync(request, ct);
        return NoContent();
    }

    [HttpPost("broadcast")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> BroadcastNotification([FromBody] BroadcastNotificationRequestDto request, CancellationToken ct)
    {
        await _notificationService.BroadcastNotificationAsync(request, ct);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var id = User.FindFirstValue(AppConstants.Jwt.ClaimUserId)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID claim not found.");
        return Guid.Parse(id);
    }
}
