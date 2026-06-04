using HospitalManagement.DataAccess.Context;
using HospitalManagement.DataAccess.Models.Emr;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.DataAccess.Repositories;

public class EmrRepository : GenericRepository<EmrRecord>, IEmrRepository
{
    public EmrRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<EmrRecord?> GetByPatientIdWithDetailsAsync(Guid patientId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(e => e.Allergies)
            .Include(e => e.MedicalHistories)
            .Include(e => e.Vitals)
            .FirstOrDefaultAsync(e => e.PatientId == patientId, ct);
    }
}
