using Microsoft.AspNetCore.Http;

namespace HospitalManagement.BusinessLogic.DTOs.Emr;

public class EmrDocumentDto
{
    public Guid Id { get; set; }
    public string BlobUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class UploadEmrDocumentRequestDto
{
    public IFormFile File { get; set; } = null!;
}
