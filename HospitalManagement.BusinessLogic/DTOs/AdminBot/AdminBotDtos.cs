namespace HospitalManagement.BusinessLogic.DTOs.AdminBot;

public class AdminBotRequestDto
{
    public string Question { get; set; } = string.Empty;
}

public class AdminBotResponseDto
{
    public string Answer { get; set; } = string.Empty;
    public string ExecutedSql { get; set; } = string.Empty;
}
