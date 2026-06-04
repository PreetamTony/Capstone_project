using HospitalManagement.DataAccess.Context;
using HospitalManagement.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.DataAccess.Repositories;

public class VisitRepository : GenericRepository<Visit>, IVisitRepository
{
    public VisitRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Visit?> GetVisitWithDetailsAsync(Guid visitId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .Include(v => v.Vitals)
            .Include(v => v.Prescriptions)
            .Include(v => v.LabReports)
            .Include(v => v.Billing)
            .FirstOrDefaultAsync(v => v.Id == visitId, ct);
    }

    public async Task<List<Visit>> GetVisitsByPatientAsync(Guid patientId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(v => v.Doctor)
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.CheckInTime)
            .ToListAsync(ct);
    }

    public async Task<List<Visit>> GetVisitsByDoctorAsync(Guid doctorId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(v => v.Patient)
            .Where(v => v.DoctorId == doctorId)
            .OrderByDescending(v => v.CheckInTime)
            .ToListAsync(ct);
    }
}
