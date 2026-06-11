using HospitalManagement.BusinessLogic.DTOs.Common;

namespace HospitalManagement.BusinessLogic.DTOs.Prescription;

public class PrescriptionResponseDto
{
    public Guid Id { get; set; }
    public Guid ConsultationId { get; set; }
    public BasicUserDto Patient { get; set; } = null!;
    public BasicUserDto Doctor { get; set; } = null!;
    
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public DateTime? DispensedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    public string? Notes { get; set; }

    public List<PrescriptionItemDto> Items { get; set; } = new List<PrescriptionItemDto>();
}
