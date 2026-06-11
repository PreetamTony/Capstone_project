namespace HospitalManagement.DataAccess.Models.Emr;

public class EmrDocument : BaseEntity
{
    public Guid EmrRecordId { get; set; }
    
    public string BlobUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public Guid UploadedBy { get; set; }

    // Navigation
    public EmrRecord EmrRecord { get; set; } = null!;
}
