namespace HospitalManagement.BusinessLogic.DTOs.Emr;

public class CreateVitalsRequestDto
{
    public Guid? VisitId { get; set; }
    public DateTime? RecordedAt { get; set; }
    public int? HeartRate { get; set; }
    public string? BloodPressure { get; set; }
    public decimal? Temperature { get; set; }
    public int? RespiratoryRate { get; set; }
    public decimal? O2Saturation { get; set; }
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
}
