using HospitalManagement.BusinessLogic.DTOs.Department;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Repositories;
using HospitalManagement.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace HospitalManagement.BusinessLogic.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _uow;
    private readonly AppDbContext _context;
    private readonly IDistributedCache _cache;
    private const string CacheKey = "all_departments";

    public DepartmentService(IUnitOfWork uow, AppDbContext context, IDistributedCache cache)
    {
        _uow = uow;
        _context = context;
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

    public async Task<List<DepartmentDoctorDto>> GetDepartmentDoctorsAsync(Guid id, CancellationToken ct = default)
    {
        var dept = await _uow.Departments.Query()
            .Include(d => d.Doctors)
            .FirstOrDefaultAsync(d => d.Id == id, ct)
            ?? throw new NotFoundException("Department", id);

        return dept.Doctors.Select(d => new DepartmentDoctorDto
        {
            DoctorId = d.Id,
            Name = $"Dr. {d.FirstName} {d.LastName}",
            Qualifications = d.Qualification,
            IsHead = dept.HeadDoctorId == d.Id
        }).ToList();
    }

    public async Task<DepartmentStatisticsDto> GetDepartmentStatisticsAsync(Guid id, CancellationToken ct = default)
    {
        var dept = await _uow.Departments.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Department", id);

        var totalDoctors = await _context.Doctors.CountAsync(d => d.DepartmentId == id, ct);
        
        var appointments = await _context.Appointments
            .Include(a => a.Doctor)
            .Where(a => a.Doctor.DepartmentId == id)
            .ToListAsync(ct);
            
        var totalAppointments = appointments.Count;
        
        var billings = await _context.Invoices
            .Where(b => b.Status == HospitalManagement.DataAccess.Models.Enums.Billing.InvoiceStatus.Paid && 
                        appointments.Select(a => a.Id).Contains(b.VisitId))
            .SumAsync(b => b.TotalAmount, ct);

        return new DepartmentStatisticsDto
        {
            TotalDoctors = totalDoctors,
            TotalAppointments = totalAppointments,
            TotalRevenue = billings
        };
    }

    public async Task AssignHeadDoctorAsync(Guid departmentId, Guid doctorId, CancellationToken ct = default)
    {
        var dept = await _uow.Departments.GetByIdAsync(departmentId, ct)
            ?? throw new NotFoundException("Department", departmentId);

        var doc = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId && d.DepartmentId == departmentId, ct);
        if (doc == null)
            throw new BusinessRuleViolationException("InvalidDoctor", "The specified doctor does not belong to this department.");

        dept.HeadDoctorId = doctorId;
        _uow.Departments.Update(dept);
        await _uow.CompleteAsync(ct);
    }
}
