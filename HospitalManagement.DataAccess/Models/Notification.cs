using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.DataAccess.Models;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
    public NotificationType Type { get; set; } = NotificationType.System;
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
    
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}
