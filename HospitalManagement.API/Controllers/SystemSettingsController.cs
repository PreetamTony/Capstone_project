using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize(Roles = AppConstants.Roles.Admin)]
[Produces("application/json")]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingService _settingService;

    public SystemSettingsController(ISystemSettingService settingService)
    {
        _settingService = settingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSettings(CancellationToken ct)
    {
        var result = await _settingService.GetAllSettingsAsync(ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> UpdateSetting(string key, [FromBody] string value, CancellationToken ct)
    {
        await _settingService.UpdateSettingAsync(key, value, ct);
        return Ok(new { success = true, message = "Setting updated successfully" });
    }
}
