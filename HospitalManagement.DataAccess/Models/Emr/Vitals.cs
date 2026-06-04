namespace HospitalManagement.DataAccess.Models.Emr;

public class Vitals : BaseEntity
{
    public Guid EmrRecordId { get; set; }
    public Guid? VisitId { get; set; } // Optional: linked to a specific visit
    
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    
    public int? HeartRate { get; set; } // bpm
    public string? BloodPressure { get; set; } // e.g., "120/80"
    public decimal? Temperature { get; set; } // Celsius or Fahrenheit depending on UI, backend stores as decimal
    public int? RespiratoryRate { get; set; } // breaths per min
    public decimal? O2Saturation { get; set; } // percentage
    public decimal? Height { get; set; } // cm
    public decimal? Weight { get; set; } // kg

    // Navigation
    public EmrRecord EmrRecord { get; set; } = null!;
    public Visit? Visit { get; set; }
}
