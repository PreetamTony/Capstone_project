using HospitalManagement.DataAccess.Models;

namespace HospitalManagement.DataAccess.Repositories;

public interface IVisitRepository : IRepository<Visit>
{
    Task<Visit?> GetVisitWithDetailsAsync(Guid visitId, CancellationToken ct = default);
    Task<List<Visit>> GetVisitsByPatientAsync(Guid patientId, CancellationToken ct = default);
    Task<List<Visit>> GetVisitsByDoctorAsync(Guid doctorId, CancellationToken ct = default);
}
