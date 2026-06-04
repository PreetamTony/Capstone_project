namespace HospitalManagement.DataAccess.Models;

public class DoctorSchedule : BaseEntity
{
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public int DayOfWeek { get; set; } // 1 = Monday, 7 = Sunday
    
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    
    public bool IsRecurring { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}
