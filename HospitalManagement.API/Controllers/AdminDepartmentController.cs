using HospitalManagement.BusinessLogic.DTOs.Department;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Presentation.Controllers;

[ApiController]
[Route("api/admin/departments")]
[Authorize(Roles = AppConstants.Roles.Admin)]
[Produces("application/json")]
public class AdminDepartmentController : ControllerBase
{
    private readonly IDepartmentService _deptService;

    public AdminDepartmentController(IDepartmentService deptService)
    {
        _deptService = deptService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _deptService.GetAllDepartmentsAsync(ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _deptService.GetDepartmentByIdAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequestDto request, CancellationToken ct)
    {
        var result = await _deptService.CreateDepartmentAsync(request, ct);
        return Created("", new { success = true, data = result });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentRequestDto request, CancellationToken ct)
    {
        await _deptService.UpdateDepartmentAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _deptService.DeleteDepartmentAsync(id, ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/doctors")]
    public async Task<IActionResult> GetDoctors(Guid id, CancellationToken ct)
    {
        var result = await _deptService.GetDepartmentDoctorsAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}/statistics")]
    public async Task<IActionResult> GetStatistics(Guid id, CancellationToken ct)
    {
        var result = await _deptService.GetDepartmentStatisticsAsync(id, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPut("{id:guid}/head-doctor/{doctorId:guid}")]
    public async Task<IActionResult> AssignHeadDoctor(Guid id, Guid doctorId, CancellationToken ct)
    {
        await _deptService.AssignHeadDoctorAsync(id, doctorId, ct);
        return NoContent();
    }
}
