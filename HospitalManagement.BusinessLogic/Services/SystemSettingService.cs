using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public class SystemSettingService : ISystemSettingService
{
    private readonly IUnitOfWork _uow;

    public SystemSettingService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<SystemSetting>> GetAllSettingsAsync(CancellationToken ct = default)
    {
        return await _uow.SystemSettings.Query().ToListAsync(ct);
    }

    public async Task<SystemSetting?> GetSettingByKeyAsync(string key, CancellationToken ct = default)
    {
        return await _uow.SystemSettings.Query().FirstOrDefaultAsync(s => s.Key == key, ct);
    }

    public async Task UpdateSettingAsync(string key, string value, CancellationToken ct = default)
    {
        var setting = await _uow.SystemSettings.Query().FirstOrDefaultAsync(s => s.Key == key, ct);
        if (setting == null)
        {
            setting = new SystemSetting
            {
                Key = key,
                Value = value,
                Description = "Auto-generated setting",
                DataType = "string"
            };
            await _uow.SystemSettings.AddAsync(setting, ct);
        }
        else
        {
            setting.Value = value;
            _uow.SystemSettings.Update(setting);
        }
        await _uow.CompleteAsync(ct);
    }
}
