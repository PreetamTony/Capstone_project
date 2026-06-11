using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HospitalManagement.API.Services.HubDispatchers;

public class QueueHubDispatcher : IQueueHubDispatcher
{
    private readonly IHubContext<QueueHub> _hubContext;

    public QueueHubDispatcher(IHubContext<QueueHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastQueueUpdateAsync(Guid doctorId, string message, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group($"DoctorQueue_{doctorId}").SendAsync("ReceiveQueueUpdate", message, ct);
    }
}
