namespace HospitalManagement.BusinessLogic.DTOs.Schedule;

public class TimeSlotDto
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
}

public class DoctorScheduleDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}

public class CreateScheduleRequestDto
{
    public List<int> DaysOfWeek { get; set; } = new List<int>();
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}

public class UpdateScheduleRequestDto
{
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}

public class ApplyLeaveRequestDto
{
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class BlockSlotRequestDto
{
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string BlockReason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
