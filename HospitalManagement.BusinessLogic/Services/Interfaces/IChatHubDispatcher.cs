using HospitalManagement.BusinessLogic.DTOs.Chat;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IChatHubDispatcher
{
    Task SendMessageAsync(Guid receiverId, ChatMessageDto message, CancellationToken ct = default);
}
