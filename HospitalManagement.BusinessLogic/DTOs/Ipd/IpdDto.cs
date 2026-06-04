namespace HospitalManagement.BusinessLogic.DTOs.Ipd;

public class WardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Capacity { get; set; }
}

public class BedDto
{
    public Guid Id { get; set; }
    public Guid WardId { get; set; }
    public string BedNumber { get; set; } = string.Empty;
    public bool IsOccupied { get; set; }
    public decimal DailyRate { get; set; }
}

public class AdmissionRecordDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public Guid BedId { get; set; }
    public string BedNumber { get; set; } = string.Empty;
    public DateTime AdmissionDate { get; set; }
    public DateTime? DischargeDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DischargeSummary { get; set; }
}

public class AdmitPatientRequestDto
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid BedId { get; set; }
}

public class DischargePatientRequestDto
{
    public string DischargeSummary { get; set; } = string.Empty;
}
