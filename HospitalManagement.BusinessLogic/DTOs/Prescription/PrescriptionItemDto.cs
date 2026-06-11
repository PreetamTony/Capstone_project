namespace HospitalManagement.BusinessLogic.DTOs.Prescription;

public class PrescriptionItemDto
{
    public Guid Id { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Strength { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public string? Instructions { get; set; }
    public int Quantity { get; set; }
    public bool IsDispensed { get; set; }
}
