using HospitalManagement.BusinessLogic.DTOs.Chat;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HospitalManagement.API.Services.HubDispatchers;

public class ChatHubDispatcher : IChatHubDispatcher
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatHubDispatcher(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendMessageAsync(Guid receiverId, ChatMessageDto message, CancellationToken ct = default)
    {
        await _hubContext.Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", message, ct);
    }
}
