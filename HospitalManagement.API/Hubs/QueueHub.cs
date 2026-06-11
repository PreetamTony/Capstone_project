using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.Presentation.Hubs;

[Authorize]
public class QueueHub : Hub
{
    private readonly ILogger<QueueHub> _logger;

    public QueueHub(ILogger<QueueHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            _logger.LogInformation("Client connected to QueueHub. ConnectionId: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during OnConnectedAsync in QueueHub");
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogWarning(exception, "Client disconnected from QueueHub with error. ConnectionId: {ConnectionId}", Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("Client disconnected from QueueHub cleanly. ConnectionId: {ConnectionId}", Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToDoctorQueue(Guid doctorId)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"DoctorQueue_{doctorId}");
            _logger.LogInformation("Connection {ConnectionId} subscribed to DoctorQueue_{DoctorId}", Context.ConnectionId, doctorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to doctor queue in QueueHub");
            throw new HubException("Failed to subscribe to queue updates.");
        }
    }

    public async Task UnsubscribeFromDoctorQueue(Guid doctorId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"DoctorQueue_{doctorId}");
            _logger.LogInformation("Connection {ConnectionId} unsubscribed from DoctorQueue_{DoctorId}", Context.ConnectionId, doctorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from doctor queue in QueueHub");
            throw new HubException("Failed to unsubscribe from queue updates.");
        }
    }
}
