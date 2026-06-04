using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HospitalManagement.BusinessLogic.Hubs;

[Authorize]
public class QueueHub : Hub
{
    // Clients will listen to "ReceiveQueueUpdate"
    // e.g., await Clients.All.SendAsync("ReceiveQueueUpdate", message);
}
