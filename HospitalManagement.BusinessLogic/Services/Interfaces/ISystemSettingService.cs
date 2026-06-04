using HospitalManagement.DataAccess.Models;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface ISystemSettingService
{
    Task<List<SystemSetting>> GetAllSettingsAsync(CancellationToken ct = default);
    Task<SystemSetting?> GetSettingByKeyAsync(string key, CancellationToken ct = default);
    Task UpdateSettingAsync(string key, string value, CancellationToken ct = default);
}
