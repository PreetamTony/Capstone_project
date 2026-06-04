namespace HospitalManagement.DataAccess.Models;

public class Bed : BaseEntity
{
    public Guid WardId { get; set; }
    public string BedNumber { get; set; } = string.Empty;
    public bool IsOccupied { get; set; }
    public decimal DailyRate { get; set; }

    public Ward Ward { get; set; } = null!;
}
