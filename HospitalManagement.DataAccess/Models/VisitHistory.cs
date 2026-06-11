using HospitalManagement.DataAccess.Models.Enums;

namespace HospitalManagement.DataAccess.Models;

public class VisitHistory : BaseEntity
{
    public Guid VisitId { get; set; }
    public Visit Visit { get; set; } = null!;

    public VisitStatus PreviousState { get; set; }
    public VisitStatus NewState { get; set; }

    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public string Reason { get; set; } = string.Empty;
}
