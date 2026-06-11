using HospitalManagement.BusinessLogic.DTOs.Admin;
using HospitalManagement.BusinessLogic.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public partial class AdminService
{
    public async Task<PagedResult<LeaveRequestDto>> GetLeaveRequestsAsync(string? status, DateTime? fromDate, DateTime? toDate, Guid? doctorId, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = _context.DoctorLeaves.Include(l => l.Doctor).AsQueryable();

        if (doctorId.HasValue)
            query = query.Where(l => l.DoctorId == doctorId.Value);

        if (fromDate.HasValue)
            query = query.Where(l => l.StartDateTime >= fromDate.Value);
            
        if (toDate.HasValue)
            query = query.Where(l => l.StartDateTime <= toDate.Value);

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (status.Equals("pending", StringComparison.OrdinalIgnoreCase))
                query = query.Where(l => !l.IsApproved && !l.IsDeleted);
            else if (status.Equals("approved", StringComparison.OrdinalIgnoreCase))
                query = query.Where(l => l.IsApproved);
            else if (status.Equals("rejected", StringComparison.OrdinalIgnoreCase) || status.Equals("cancelled", StringComparison.OrdinalIgnoreCase))
                query = query.Where(l => l.IsDeleted); 
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LeaveRequestDto
            {
                Id = l.Id,
                DoctorId = l.DoctorId,
                DoctorName = $"Dr. {l.Doctor.FirstName} {l.Doctor.LastName}",
                StartDate = l.StartDateTime,
                EndDate = l.EndDateTime,
                Reason = l.Reason,
                Status = l.IsDeleted ? "Rejected" : (l.IsApproved ? "Approved" : "Pending"),
                CreatedAt = l.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<LeaveRequestDto>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<LeaveRequestDto> GetLeaveRequestByIdAsync(Guid id, CancellationToken ct = default)
    {
        var l = await _context.DoctorLeaves.Include(dl => dl.Doctor)
            .FirstOrDefaultAsync(dl => dl.Id == id, ct)
            ?? throw new HospitalManagement.DataAccess.Exceptions.NotFoundException("DoctorLeave", id);

        return new LeaveRequestDto
        {
            Id = l.Id,
            DoctorId = l.DoctorId,
            DoctorName = $"Dr. {l.Doctor.FirstName} {l.Doctor.LastName}",
            StartDate = l.StartDateTime,
            EndDate = l.EndDateTime,
            Reason = l.Reason,
            Status = l.IsDeleted ? "Rejected" : (l.IsApproved ? "Approved" : "Pending"),
            CreatedAt = l.CreatedAt
        };
    }

    public async Task ApproveLeaveRequestAsync(Guid id, string? notes, CancellationToken ct = default)
    {
        var l = await _context.DoctorLeaves.FirstOrDefaultAsync(dl => dl.Id == id, ct)
            ?? throw new HospitalManagement.DataAccess.Exceptions.NotFoundException("DoctorLeave", id);

        l.IsApproved = true;
        l.AdminNotes = notes;
        l.ApprovedBy = _currentUserService.UserId;
        _context.DoctorLeaves.Update(l);
        await _context.SaveChangesAsync(ct);
    }

    public async Task RejectLeaveRequestAsync(Guid id, string reason, CancellationToken ct = default)
    {
        var l = await _context.DoctorLeaves.FirstOrDefaultAsync(dl => dl.Id == id, ct)
            ?? throw new HospitalManagement.DataAccess.Exceptions.NotFoundException("DoctorLeave", id);

        l.IsApproved = false;
        l.IsDeleted = true; // Hard mark as rejected
        l.AdminNotes = reason; // Store rejection reason
        l.DeletedAt = DateTime.UtcNow;
        l.DeletedBy = _currentUserService.UserId;
        _context.DoctorLeaves.Update(l);
        await _context.SaveChangesAsync(ct);
    }

    public async Task CancelLeaveRequestAsync(Guid id, CancellationToken ct = default)
    {
        await RejectLeaveRequestAsync(id, "Cancelled", ct);
    }
}
