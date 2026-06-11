using HospitalManagement.BusinessLogic.DTOs.AdminBot;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppConstants.Roles.Admin)]
public class AdminBotController : ControllerBase
{
    private readonly IAdminBotService _adminBotService;

    public AdminBotController(IAdminBotService adminBotService)
    {
        _adminBotService = adminBotService;
    }

    [HttpPost("query")]
    public async Task<ActionResult<AdminBotResponseDto>> QueryDatabase([FromBody] AdminBotRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("Question cannot be empty.");
        }

        var result = await _adminBotService.QueryDatabaseAsync(request.Question, ct);
        return Ok(result);
    }
}
