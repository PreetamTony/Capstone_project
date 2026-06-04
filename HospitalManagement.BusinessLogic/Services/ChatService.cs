using HospitalManagement.BusinessLogic.Hubs;
using HospitalManagement.BusinessLogic.DTOs.Chat;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public class ChatService : IChatService
{
    private readonly IUnitOfWork _uow;
    private readonly IHubContext<ChatHub> _chatHub;

    public ChatService(IUnitOfWork uow, IHubContext<ChatHub> chatHub)
    {
        _uow = uow;
        _chatHub = chatHub;
    }

    public async Task<ChatMessageDto> SendMessageAsync(Guid senderId, SendMessageRequestDto request, CancellationToken ct = default)
    {
        var sender = await _uow.Users.GetByIdAsync(senderId, ct) ?? throw new NotFoundException("User", senderId);
        var receiver = await _uow.Users.GetByIdAsync(request.ReceiverId, ct) ?? throw new NotFoundException("User", request.ReceiverId);

        var msg = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = request.ReceiverId,
            Content = request.Content,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        await _uow.ChatMessages.AddAsync(msg, ct);
        await _uow.CompleteAsync(ct);

        var dto = new ChatMessageDto
        {
            Id = msg.Id,
            SenderId = msg.SenderId,
            SenderName = sender.Email,
            ReceiverId = msg.ReceiverId,
            ReceiverName = receiver.Email,
            Content = msg.Content,
            SentAt = msg.SentAt,
            IsRead = msg.IsRead
        };

        // Broadcast to the specific receiver user
        await _chatHub.Clients.User(receiver.Id.ToString()).SendAsync("ReceiveMessage", dto, ct);

        return dto;
    }

    public async Task<List<ChatMessageDto>> GetChatHistoryAsync(Guid userId1, Guid userId2, CancellationToken ct = default)
    {
        var messages = await _uow.ChatMessages.Query()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                        (m.SenderId == userId2 && m.ReceiverId == userId1))
            .OrderBy(m => m.SentAt)
            .ToListAsync(ct);

        return messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderName = m.Sender.Email,
            ReceiverId = m.ReceiverId,
            ReceiverName = m.Receiver.Email,
            Content = m.Content,
            SentAt = m.SentAt,
            IsRead = m.IsRead
        }).ToList();
    }

    public async Task MarkAsReadAsync(Guid messageId, Guid receiverId, CancellationToken ct = default)
    {
        var msg = await _uow.ChatMessages.GetByIdAsync(messageId, ct) ?? throw new NotFoundException("Message", messageId);
        
        if (msg.ReceiverId != receiverId)
            throw new System.UnauthorizedAccessException("Cannot mark another user's message as read.");

        msg.IsRead = true;
        _uow.ChatMessages.Update(msg);
        await _uow.CompleteAsync(ct);
    }
}
