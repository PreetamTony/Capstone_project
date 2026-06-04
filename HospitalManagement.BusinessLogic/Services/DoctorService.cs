using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Doctor;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace HospitalManagement.BusinessLogic.Services;

public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _uow;
    private readonly IDistributedCache _cache;

    public DoctorService(IUnitOfWork uow, IDistributedCache cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<PagedResult<DoctorResponseDto>> GetAllAsync(DoctorPaginationFilter filter, CancellationToken ct = default)
    {
        var cacheKey = $"Doctors_{filter.PageNumber}_{filter.PageSize}_{filter.SearchTerm}_{filter.DepartmentId}";
        
        var cachedData = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<PagedResult<DoctorResponseDto>>(cachedData)!;
        }

        var query = _uow.Doctors.Query()
            .Include(d => d.User)
            .AsQueryable();

        if (filter.DepartmentId.HasValue)
            query = query.Where(d => d.DepartmentId == filter.DepartmentId);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(d => 
                d.FirstName.ToLower().Contains(term) || 
                d.LastName.ToLower().Contains(term) ||
                d.Specialization.ToLower().Contains(term));
        }

        var total = await query.CountAsync(ct);
        var doctors = await query
            .Include(d => d.Department)
            .OrderBy(d => d.LastName)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var today = (int)DateTime.UtcNow.DayOfWeek;
        // Fix Sunday = 7 mapping if needed. .NET DayOfWeek is Sunday = 0
        var currentDay = today == 0 ? 7 : today;
        
        var doctorIds = doctors.Select(d => d.Id).ToList();
        var schedules = await _uow.DoctorSchedules.Query()
            .Where(s => doctorIds.Contains(s.DoctorId) && s.DayOfWeek == currentDay)
            .ToListAsync(ct);

        var dtos = doctors.Select(d => 
        {
            var isAvailableToday = schedules.Any(s => s.DoctorId == d.Id);
            return MapToDto(d, isAvailableToday);
        }).ToList();
        var result = PagedResult<DoctorResponseDto>.Create(dtos, total, filter.PageNumber, filter.PageSize);
        
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        }, ct);
        
        return result;
    }

    public async Task<DoctorResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.Query()
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.Id == id, ct)
            ?? throw new NotFoundException("Doctor", id);

        var today = (int)DateTime.UtcNow.DayOfWeek;
        var currentDay = today == 0 ? 7 : today;
        var isAvailable = await _uow.DoctorSchedules.Query()
            .AnyAsync(s => s.DoctorId == id && s.DayOfWeek == currentDay, ct);

        return MapToDto(doctor, isAvailable);
    }

    private static DoctorResponseDto MapToDto(Doctor d, bool isAvailable) => new()
    {
        Id = d.Id,
        UserId = d.UserId,
        FullName = d.FullName,
        FirstName = d.FirstName,
        LastName = d.LastName,
        Specialization = d.Specialization,
        LicenseNumber = d.LicenseNumber,
        YearsOfExperience = d.ExperienceYears,
        DepartmentId = d.DepartmentId,
        DepartmentName = d.Department?.Name ?? string.Empty,
        ConsultationFee = d.ConsultationFee,
        MaxPatientsPerDay = d.MaxPatientsPerDay,
        AverageConsultationMinutes = d.AverageConsultationMinutes,
        IsAvailable = isAvailable,
        Rating = d.Rating,
        Email = d.User?.Email ?? string.Empty,
        PhoneNumber = d.User?.PhoneNumber ?? string.Empty
    };
}
