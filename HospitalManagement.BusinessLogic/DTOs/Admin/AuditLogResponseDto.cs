namespace HospitalManagement.BusinessLogic.DTOs.Admin;

public class AuditLogResponseDto
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserRole { get; set; }
    public string? PerformedByName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string[]? ChangedFields { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }
}

public class AuditLogDetailResponseDto : AuditLogResponseDto
{
    public Dictionary<string, ChangeDetailDto>? Changes { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
