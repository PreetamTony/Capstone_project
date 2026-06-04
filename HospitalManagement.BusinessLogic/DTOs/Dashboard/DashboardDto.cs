namespace HospitalManagement.BusinessLogic.DTOs.Dashboard;

public class AdminDashboardDto
{
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int AppointmentsToday { get; set; }
    public int PendingLabReports { get; set; }
    public decimal TotalRevenueToday { get; set; }
}

public class DoctorDashboardDto
{
    public int PatientsToday { get; set; }
    public int PendingConsultations { get; set; }
    public int CompletedConsultations { get; set; }
    public int CancelledAppointments { get; set; }
}

public class PatientDashboardDto
{
    public int UpcomingAppointments { get; set; }
    public int UnpaidBills { get; set; }
    public int NewLabReports { get; set; }
}
