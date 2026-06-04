using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.BusinessLogic.DTOs.Patient;

public class CreateDocumentRequestDto
{
    [Required]
    public Guid PatientId { get; set; }

    [Required]
    public string DocumentType { get; set; } = string.Empty;

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string FileUrl { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }
}

public class DocumentResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}
