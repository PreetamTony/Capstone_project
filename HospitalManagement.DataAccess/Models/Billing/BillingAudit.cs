namespace HospitalManagement.DataAccess.Models.Billing;

public class BillingAudit : BaseEntity
{
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Guid EntityId { get; set; }
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
}
