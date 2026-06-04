namespace HospitalManagement.BusinessLogic.DTOs.Admin;

public class DailySummaryDto
{
    public DateTime Date { get; set; }
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int NoShows { get; set; }
    public int NewPatients { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal ConsultationRevenue { get; set; }
    public decimal LabRevenue { get; set; }
    public decimal PharmacyRevenue { get; set; }
    public int InsuranceClaims { get; set; }
    public decimal InsuranceAmount { get; set; }
    public decimal PendingPayments { get; set; }
    public decimal RefundsProcessed { get; set; }

    public int TotalDoctorsAvailable { get; set; }
    public int PendingBills { get; set; }
    public int LabReportsPending { get; set; }
    public int AdmittedPatients { get; set; }

    public double AppointmentUtilizationRate { get; set; }
    public int ReturningPatients { get; set; }
    public int TotalPatientsServed { get; set; }
    public int WalkInPatients { get; set; }
    public int Teleconsultations { get; set; }

    public double AverageWaitTimeMinutes { get; set; }
    public double AverageConsultationTimeMinutes { get; set; }
    
    public List<DoctorUtilizationDto> DoctorUtilization { get; set; } = new();
    public Dictionary<string, DepartmentMetricsDto> DepartmentMetrics { get; set; } = new();
    
    public string PeakHour { get; set; } = string.Empty;
    public string SlowestHour { get; set; } = string.Empty;
    
    public PreviousDayComparisonDto VsPreviousDay { get; set; } = new();
}

public class DoctorUtilizationDto
{
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int PatientsSeen { get; set; }
    public double AverageWaitTime { get; set; }
    public decimal Revenue { get; set; }
}

public class DepartmentMetricsDto
{
    public int Appointments { get; set; }
    public decimal Revenue { get; set; }
    public double WaitTime { get; set; }
}

public class PreviousDayComparisonDto
{
    public double AppointmentsChange { get; set; }
    public double RevenueChange { get; set; }
}
