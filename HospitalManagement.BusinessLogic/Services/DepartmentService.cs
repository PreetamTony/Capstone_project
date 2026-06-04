using HospitalManagement.BusinessLogic.DTOs.Department;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace HospitalManagement.BusinessLogic.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _uow;
    private readonly IDistributedCache _cache;
    private const string CacheKey = "AllDepartments";

    public DepartmentService(IUnitOfWork uow, IDistributedCache cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<List<DepartmentDto>> GetAllDepartmentsAsync(CancellationToken ct = default)
    {
        var cachedData = await _cache.GetStringAsync(CacheKey, ct);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<List<DepartmentDto>>(cachedData) ?? new List<DepartmentDto>();
        }

        var deps = await _uow.Departments.GetAllAsync(ct);
        var dtos = deps.Select(d => new DepartmentDto
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description
        }).ToList();

        await _cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(dtos), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        }, ct);

        return dtos;
    }

    public async Task<DepartmentDto> GetDepartmentByIdAsync(Guid id, CancellationToken ct = default)
    {
        var d = await _uow.Departments.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Department", id);

        return new DepartmentDto
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description
        };
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequestDto request, CancellationToken ct = default)
    {
        var existing = await _uow.Departments.FirstOrDefaultAsync(d => d.Name.ToLower() == request.Name.ToLower(), ct);
        if (existing != null)
            throw new BusinessRuleViolationException("DuplicateDepartment", "A department with this name already exists.");

        var dept = new Department
        {
            Name = request.Name,
            Description = request.Description
        };

        await _uow.Departments.AddAsync(dept, ct);
        await _uow.CompleteAsync(ct);
        
        await _cache.RemoveAsync(CacheKey, ct);
        
        return new DepartmentDto
        {
            Id = dept.Id,
            Name = dept.Name,
            Description = dept.Description
        };
    }

    public async Task UpdateDepartmentAsync(Guid id, UpdateDepartmentRequestDto request, CancellationToken ct = default)
    {
        var dept = await _uow.Departments.GetByIdAsync(id, ct) ?? throw new NotFoundException("Department", id);

        dept.Name = request.Name;
        dept.Description = request.Description;

        _uow.Departments.Update(dept);
        await _uow.CompleteAsync(ct);

        await _cache.RemoveAsync(CacheKey, ct);
    }

    public async Task<bool> DeleteDepartmentAsync(Guid id, CancellationToken ct = default)
    {
        var d = await _uow.Departments.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Department", id);

        _uow.Departments.Delete(d);
        await _uow.CompleteAsync(ct);
        
        await _cache.RemoveAsync(CacheKey, ct);
        
        return true;
    }
}
