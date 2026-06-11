using System.Security.Claims;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.Presentation.Hubs;

[Authorize]
public class AppointmentHub : Hub
{
    private readonly ILogger<AppointmentHub> _logger;

    public AppointmentHub(ILogger<AppointmentHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = Context.User?.FindFirstValue(AppConstants.Jwt.ClaimUserId) 
                         ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation("Client connected to AppointmentHub. ConnectionId: {ConnectionId}, UserId: {UserId}", Context.ConnectionId, userId);

            // Add to general updates group or specific department groups based on role
            if (Context.User?.IsInRole("Receptionist") == true || Context.User?.IsInRole("Admin") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "ReceptionStaff");
            }

            if (!string.IsNullOrEmpty(userId))
            {
                // Patient groups and Doctor groups
                if (Context.User?.IsInRole(AppConstants.Roles.Patient) == true)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Patient_{userId}");
                }
                else if (Context.User?.IsInRole(AppConstants.Roles.Doctor) == true)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Doctor_{userId}");
                }
            }

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during OnConnectedAsync in AppointmentHub");
            throw; // SignalR will close connection if an error occurs here
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogWarning(exception, "Client disconnected from AppointmentHub with error. ConnectionId: {ConnectionId}", Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("Client disconnected from AppointmentHub cleanly. ConnectionId: {ConnectionId}", Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToAppointment(Guid appointmentId)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Appointment_{appointmentId}");
            _logger.LogInformation("Connection {ConnectionId} subscribed to Appointment_{AppointmentId}", Context.ConnectionId, appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to appointment in AppointmentHub");
            throw new HubException("Failed to subscribe to appointment updates.");
        }
    }

    public async Task UnsubscribeFromAppointment(Guid appointmentId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Appointment_{appointmentId}");
            _logger.LogInformation("Connection {ConnectionId} unsubscribed from Appointment_{AppointmentId}", Context.ConnectionId, appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from appointment in AppointmentHub");
            throw new HubException("Failed to unsubscribe from appointment updates.");
        }
    }
}
