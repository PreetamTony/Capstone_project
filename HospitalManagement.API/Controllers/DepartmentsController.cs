using HospitalManagement.BusinessLogic.DTOs.Department;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<DepartmentDto>>> GetAllDepartments(CancellationToken ct)
    {
        var result = await _departmentService.GetAllDepartmentsAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<DepartmentDto>> GetDepartmentById(Guid id, CancellationToken ct)
    {
        var result = await _departmentService.GetDepartmentByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] CreateDepartmentRequestDto request, CancellationToken ct)
    {
        var result = await _departmentService.CreateDepartmentAsync(request, ct);
        return CreatedAtAction(nameof(GetDepartmentById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentRequestDto request, CancellationToken ct)
    {
        await _departmentService.UpdateDepartmentAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> DeleteDepartment(Guid id, CancellationToken ct)
    {
        await _departmentService.DeleteDepartmentAsync(id, ct);
        return NoContent();
    }
}
