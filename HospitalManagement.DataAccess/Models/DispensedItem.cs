namespace HospitalManagement.DataAccess.Models;

public class DispensedItem : BaseEntity
{
    public Guid DispensationRecordId { get; set; }
    public Guid MedicationId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public DispensationRecord Record { get; set; } = null!;
    public MedicationInventory Medication { get; set; } = null!;
}
