namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IQueueHubDispatcher
{
    Task BroadcastQueueUpdateAsync(Guid doctorId, string message, CancellationToken ct = default);
}
