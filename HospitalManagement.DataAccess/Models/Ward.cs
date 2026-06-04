namespace HospitalManagement.DataAccess.Models;

public class Ward : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g. "General", "ICU", "Maternity"
    public int Capacity { get; set; }

    public ICollection<Bed> Beds { get; set; } = new List<Bed>();
}
