using HospitalManagement.BusinessLogic.DTOs.AdminBot;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IAdminBotService
{
    Task<AdminBotResponseDto> QueryDatabaseAsync(string question, CancellationToken ct = default);
}
