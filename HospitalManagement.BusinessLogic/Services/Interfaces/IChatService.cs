using HospitalManagement.BusinessLogic.DTOs.Chat;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IChatService
{
    Task<ChatMessageDto> SendMessageAsync(Guid senderId, SendMessageRequestDto request, CancellationToken ct = default);
    Task<List<ChatMessageDto>> GetChatHistoryAsync(Guid userId1, Guid userId2, CancellationToken ct = default);
    Task MarkAsReadAsync(Guid messageId, Guid receiverId, CancellationToken ct = default);
}
