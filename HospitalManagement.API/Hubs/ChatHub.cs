using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.Presentation.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = Context.UserIdentifier;
            _logger.LogInformation("Client connected to ChatHub. ConnectionId: {ConnectionId}, UserId: {UserId}", Context.ConnectionId, userId);
            
            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during OnConnectedAsync in ChatHub");
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogWarning(exception, "Client disconnected from ChatHub with error. ConnectionId: {ConnectionId}", Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("Client disconnected from ChatHub cleanly. ConnectionId: {ConnectionId}", Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}
