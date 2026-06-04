namespace HospitalManagement.DataAccess.Models;

public class BlockedSlot : BaseEntity
{
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    
    public string BlockReason { get; set; } = string.Empty; // Surgery, Meeting, Personal
    public string Description { get; set; } = string.Empty;
}
