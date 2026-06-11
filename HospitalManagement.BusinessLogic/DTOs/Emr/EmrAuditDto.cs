namespace HospitalManagement.BusinessLogic.DTOs.Emr;

public class EmrAuditDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
}
