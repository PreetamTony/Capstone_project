using HospitalManagement.BusinessLogic.DTOs.Chat;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("send")]
    public async Task<ActionResult<ChatMessageDto>> SendMessage([FromBody] SendMessageRequestDto request, CancellationToken ct)
    {
        var senderId = GetCurrentUserId();
        var result = await _chatService.SendMessageAsync(senderId, request, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("history/{userId}")]
    public async Task<ActionResult<List<ChatMessageDto>>> GetChatHistory(Guid userId, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _chatService.GetChatHistoryAsync(currentUserId, userId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("read/{messageId}")]
    public async Task<ActionResult> MarkAsRead(Guid messageId, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        await _chatService.MarkAsReadAsync(messageId, currentUserId, ct);
        return Ok(new { success = true });
    }

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
    }
}
