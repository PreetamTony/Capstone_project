using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.DataAccess.Models;

/// <summary>
/// Immutable audit trail for every Create/Update/Delete operation across all entities.
/// </summary>
public class AuditLog
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserRole { get; set; }
    public AuditActionType Action { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string? OldValues { get; set; }                    // JSONB
    public string? NewValues { get; set; }                    // JSONB
    public string[]? ChangedFields { get; set; }              // JSONB array or string[]
    public string? CorrelationId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? PerformedByName { get; set; }
    public bool IsArchived { get; set; } = false;
}
