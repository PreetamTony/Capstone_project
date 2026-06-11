namespace HospitalManagement.BusinessLogic.DTOs.Emr;

public class EmergencyInfoDto
{
    public string? BloodGroup { get; set; }
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public string? EmergencyContactRelationship { get; set; }
    public bool OrganDonorFlag { get; set; }
}

public class UpdateEmergencyInfoRequestDto
{
    public string? BloodGroup { get; set; }
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public string? EmergencyContactRelationship { get; set; }
    public bool OrganDonorFlag { get; set; }
}
