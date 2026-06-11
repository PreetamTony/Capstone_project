namespace HospitalManagement.BusinessLogic.DTOs.Prescription;

public class CreatePrescriptionRequestDto
{
    public Guid ConsultationId { get; set; }
    public string? Notes { get; set; }
}
