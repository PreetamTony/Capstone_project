using HospitalManagement.BusinessLogic.DTOs.Common;

namespace HospitalManagement.BusinessLogic.DTOs.Prescription;

public class PrescriptionSummaryDto
{
    public Guid Id { get; set; }
    public Guid ConsultationId { get; set; }
    public BasicUserDto Patient { get; set; } = null!;
    public BasicUserDto Doctor { get; set; } = null!;
    
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ItemCount { get; set; }
}
