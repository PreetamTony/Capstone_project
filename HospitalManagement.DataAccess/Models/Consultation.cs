namespace HospitalManagement.DataAccess.Models;

public class Consultation : BaseEntity
{
    public Guid VisitId { get; set; }
    public Visit Visit { get; set; } = null!;

    public string Symptoms { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public HospitalManagement.DataAccess.Models.Enums.ConsultationStatus Status { get; set; } = HospitalManagement.DataAccess.Models.Enums.ConsultationStatus.Draft;
}
