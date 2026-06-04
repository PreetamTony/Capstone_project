using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HospitalManagement.BusinessLogic.Hubs;

[Authorize]
public class ChatHub : Hub
{
    // Clients will listen to "ReceiveMessage"
    // e.g., await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message);
}
