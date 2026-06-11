namespace HospitalManagement.DataAccess.Models;

public class DoctorLeave : BaseEntity
{
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public string LeaveType { get; set; } = string.Empty; // Sick, Vacation, Conference
    
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    
    public string Reason { get; set; } = string.Empty;
    
    public bool IsApproved { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string? AdminNotes { get; set; }
}
