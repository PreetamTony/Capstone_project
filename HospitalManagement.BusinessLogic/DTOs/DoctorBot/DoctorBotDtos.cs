namespace HospitalManagement.BusinessLogic.DTOs.DoctorBot;

public class DoctorBotRequestDto
{
    public string Question { get; set; } = string.Empty;
}

public class DoctorBotResponseDto
{
    public string Answer { get; set; } = string.Empty;
}
