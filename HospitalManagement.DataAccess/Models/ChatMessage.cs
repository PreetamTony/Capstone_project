namespace HospitalManagement.DataAccess.Models;

public class ChatMessage : BaseEntity
{
    public Guid SenderId { get; set; } // UserId of the sender
    public Guid ReceiverId { get; set; } // UserId of the receiver
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }

    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}
