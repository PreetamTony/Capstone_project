using System.Text;
using HospitalManagement.BusinessLogic.DTOs.Admin;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public partial class AdminService
{
    public async Task<PagedResult<DoctorSummaryDto>> GetDoctorsAsync(Guid? departmentId, bool? isActive, string? search, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = _uow.Doctors.Query().Include(d => d.Department).Include(d => d.User).AsQueryable();

        if (departmentId.HasValue)
            query = query.Where(d => d.DepartmentId == departmentId.Value);

        if (isActive.HasValue)
            query = query.Where(d => d.User.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(d => d.FirstName.ToLower().Contains(searchLower) || d.LastName.ToLower().Contains(searchLower));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderBy(d => d.LastName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DoctorSummaryDto
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                Specialization = d.Specialization,
                DepartmentName = d.Department != null ? d.Department.Name : null,
                ConsultationFee = d.ConsultationFee,
                IsActive = d.User.IsActive,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<DoctorSummaryDto>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<DoctorDetailDto> GetDoctorByIdAsync(Guid doctorId, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.Query()
            .Include(d => d.Department)
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == doctorId, ct)
            ?? throw new HospitalManagement.DataAccess.Exceptions.NotFoundException("Doctor", doctorId);

        var pendingLeave = await _context.DoctorLeaves.CountAsync(l => l.DoctorId == doctorId && !l.IsApproved, ct);
        
        var revenue = await _context.Invoices
            .Include(b => b.Visit)
            .ThenInclude(v => v.Appointment)
            .Where(b => b.Status == HospitalManagement.DataAccess.Models.Enums.Billing.InvoiceStatus.Paid && b.Visit.Appointment.DoctorId == doctorId)
            .SumAsync(b => b.TotalAmount, ct);

        return new DoctorDetailDto
        {
            Id = doctor.Id,
            FirstName = doctor.FirstName,
            LastName = doctor.LastName,
            Specialization = doctor.Specialization,
            DepartmentName = doctor.Department?.Name,
            ConsultationFee = doctor.ConsultationFee,
            IsActive = doctor.User.IsActive,
            CreatedAt = doctor.CreatedAt,
            Qualification = doctor.Qualification,
            ExperienceYears = doctor.ExperienceYears,
            PendingLeaveRequests = pendingLeave,
            TotalRevenueGenerated = revenue
        };
    }

    public async Task UpdateDoctorAsync(Guid doctorId, UpdateDoctorRequestDto request, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(doctorId, ct)
            ?? throw new HospitalManagement.DataAccess.Exceptions.NotFoundException("Doctor", doctorId);

        doctor.FirstName = request.FirstName;
        doctor.LastName = request.LastName;
        doctor.Specialization = request.Specialization;
        doctor.Qualification = request.Qualification;
        doctor.ExperienceYears = request.ExperienceYears;
        doctor.ConsultationFee = request.ConsultationFee;
        doctor.DepartmentId = request.DepartmentId;
        
        var user = await _uow.Users.GetByIdAsync(doctor.UserId, ct);
        if (user != null)
        {
            user.IsActive = request.IsActive;
            _uow.Users.Update(user);
        }

        _uow.Doctors.Update(doctor);
        await _uow.CompleteAsync(ct);
    }

    public async Task ArchiveDoctorAsync(Guid doctorId, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(doctorId, ct)
            ?? throw new HospitalManagement.DataAccess.Exceptions.NotFoundException("Doctor", doctorId);

        doctor.IsDeleted = true;
        doctor.DeletedAt = DateTime.UtcNow;
        doctor.DeletedBy = _currentUserService.UserId;
        
        _uow.Doctors.Update(doctor);
        await _uow.CompleteAsync(ct);
    }

    public async Task<byte[]> ExportDoctorsCsvAsync(CancellationToken ct = default)
    {
        var doctors = await _uow.Doctors.Query().Include(d => d.Department).Include(d => d.User).OrderBy(d => d.LastName).ToListAsync(ct);
        var builder = new StringBuilder();
        builder.AppendLine("Id,FirstName,LastName,Specialization,Department,ConsultationFee,IsActive");
        foreach (var d in doctors)
        {
            builder.AppendLine($"{d.Id},{d.FirstName},{d.LastName},{d.Specialization},{d.Department?.Name},{d.ConsultationFee},{d.User.IsActive}");
        }
        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    public async Task<DoctorStatsDto> GetDoctorStatsAsync(CancellationToken ct = default)
    {
        var total = await _uow.Doctors.Query().CountAsync(ct);
        var active = await _uow.Doctors.Query().Include(d => d.User).CountAsync(d => d.User.IsActive, ct);
        
        var today = DateTime.UtcNow.Date;
        var onLeave = await _context.DoctorLeaves
            .Where(l => l.StartDateTime.Date <= today && l.EndDateTime.Date >= today && l.IsApproved)
            .Select(l => l.DoctorId)
            .Distinct()
            .CountAsync(ct);

        var doctorsByDept = await _uow.Doctors.Query()
            .Include(d => d.Department)
            .GroupBy(d => d.Department != null ? d.Department.Name : "Unassigned")
            .Select(g => new { Dept = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Dept, g => g.Count, ct);

        var avgFee = await _uow.Doctors.Query().AverageAsync(d => (decimal?)d.ConsultationFee, ct) ?? 0;

        var topByPatients = await _context.Appointments
            .Include(a => a.Doctor)
            .GroupBy(a => a.Doctor)
            .Select(g => new TopDoctorDto
            {
                DoctorId = g.Key.Id,
                Name = $"Dr. {g.Key.FirstName} {g.Key.LastName}",
                ValueInt = g.Count(),
                ValueDecimal = 0
            })
            .OrderByDescending(t => t.ValueInt)
            .Take(5)
            .ToListAsync(ct);

        var topByRevenue = await _context.Invoices
            .Include(b => b.Visit).ThenInclude(v => v.Doctor)
            .Where(b => b.Status == HospitalManagement.DataAccess.Models.Enums.Billing.InvoiceStatus.Paid && b.Visit != null && b.Visit.Doctor != null)
            .GroupBy(b => b.Visit!.Doctor)
            .Select(g => new TopDoctorDto
            {
                DoctorId = g.Key.Id,
                Name = $"Dr. {g.Key.FirstName} {g.Key.LastName}",
                ValueInt = 0,
                ValueDecimal = g.Sum(b => b.TotalAmount)
            })
            .OrderByDescending(t => t.ValueDecimal)
            .Take(5)
            .ToListAsync(ct);

        return new DoctorStatsDto
        {
            TotalDoctors = total,
            ActiveDoctors = active,
            DoctorsOnLeave = onLeave,
            DoctorsByDepartment = doctorsByDept,
            AverageConsultationFee = Math.Round(avgFee, 2),
            TopDoctorsByPatients = topByPatients,
            TopDoctorsByRevenue = topByRevenue
        };
    }
}
