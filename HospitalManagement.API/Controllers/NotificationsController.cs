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

    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        await _notificationService.MarkAsReadAsync(id, GetCurrentUserId(), ct);
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

    private Guid GetCurrentUserId()
    {
        var id = User.FindFirstValue(AppConstants.Jwt.ClaimUserId)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID claim not found.");
        return Guid.Parse(id);
    }
}
