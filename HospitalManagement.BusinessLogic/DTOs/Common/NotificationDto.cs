namespace HospitalManagement.BusinessLogic.DTOs.Common;

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateNotificationRequestDto
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
    public string Type { get; set; } = "System";
    public string Priority { get; set; } = "Normal";
    public string Channel { get; set; } = "InApp";
    
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
}

public class BroadcastNotificationRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = "High";
}
