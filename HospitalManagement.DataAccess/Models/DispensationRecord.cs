namespace HospitalManagement.DataAccess.Models;

public class DispensationRecord : BaseEntity
{
    public Guid PrescriptionId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime DispensedAt { get; set; }
    public decimal TotalCost { get; set; }

    public Prescription Prescription { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public ICollection<DispensedItem> Items { get; set; } = new List<DispensedItem>();
}
