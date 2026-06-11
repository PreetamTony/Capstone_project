using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.BusinessLogic.DTOs.Appointment;

namespace HospitalManagement.BusinessLogic.DTOs.Visit;

public class VisitDetailsDto
{
    public Guid Id { get; set; }
    public string VisitNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string VisitType { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public DateTime? DischargeTime { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? Notes { get; set; }
    public string? QueueNumber { get; set; }
    public string? RoomNumber { get; set; }

    public VisitPatientSummary Patient { get; set; } = new();
    public VisitDoctorSummary Doctor { get; set; } = new();
    public AppointmentSummaryDto? Appointment { get; set; }
    public VisitConsultationSummary? Consultation { get; set; }
    
    public List<VisitPrescriptionSummary> Prescriptions { get; set; } = new();
    public List<VisitLabReportSummary> LabReports { get; set; } = new();
    public VisitBillingSummary? Billing { get; set; }
}

public class VisitPatientSummary
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? BloodGroup { get; set; }
}

public class VisitDoctorSummary
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
}

public class VisitConsultationSummary
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Diagnosis { get; set; }
    public string? Recommendations { get; set; }
}

public class VisitPrescriptionSummary
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public List<VisitPrescriptionItemSummary> Items { get; set; } = new();
}

public class VisitPrescriptionItemSummary
{
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
}

public class VisitLabReportSummary
{
    public Guid Id { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class VisitBillingSummary
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}
