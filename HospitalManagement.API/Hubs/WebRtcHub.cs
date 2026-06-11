using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.Presentation.Hubs;

[Authorize]
public class WebRtcHub : Hub
{
    private readonly ILogger<WebRtcHub> _logger;

    public WebRtcHub(ILogger<WebRtcHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinVideoRoom(string appointmentId)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Video_{appointmentId}");
            _logger.LogInformation("User {UserId} joined video room {AppointmentId}", Context.UserIdentifier, appointmentId);
            
            // Notify others in the room that a peer joined
            await Clients.OthersInGroup($"Video_{appointmentId}").SendAsync("PeerJoined", Context.UserIdentifier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining video room {AppointmentId}", appointmentId);
            throw new HubException("Failed to join the video room.");
        }
    }

    public async Task LeaveVideoRoom(string appointmentId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Video_{appointmentId}");
            _logger.LogInformation("User {UserId} left video room {AppointmentId}", Context.UserIdentifier, appointmentId);
            
            await Clients.OthersInGroup($"Video_{appointmentId}").SendAsync("PeerLeft", Context.UserIdentifier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving video room {AppointmentId}", appointmentId);
            throw new HubException("Failed to leave the video room.");
        }
    }

    public async Task SendOffer(string appointmentId, string offer)
    {
        // Route the SDP Offer to the other peer in the room
        await Clients.OthersInGroup($"Video_{appointmentId}").SendAsync("ReceiveOffer", Context.UserIdentifier, offer);
    }

    public async Task SendAnswer(string appointmentId, string answer)
    {
        // Route the SDP Answer back to the caller
        await Clients.OthersInGroup($"Video_{appointmentId}").SendAsync("ReceiveAnswer", Context.UserIdentifier, answer);
    }

    public async Task SendIceCandidate(string appointmentId, string candidate)
    {
        // Exchange ICE candidates for NAT traversal
        await Clients.OthersInGroup($"Video_{appointmentId}").SendAsync("ReceiveIceCandidate", Context.UserIdentifier, candidate);
    }
}
