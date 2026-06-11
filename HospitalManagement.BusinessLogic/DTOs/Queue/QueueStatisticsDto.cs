namespace HospitalManagement.BusinessLogic.DTOs.Queue;

public class QueueStatisticsDto
{
    public int AverageWaitTime { get; set; }
    public int PatientsWaiting { get; set; }
    public int PatientsServedToday { get; set; }
    public int NoShows { get; set; }
}
