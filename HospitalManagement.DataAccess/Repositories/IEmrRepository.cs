using HospitalManagement.DataAccess.Models.Emr;

namespace HospitalManagement.DataAccess.Repositories;

public interface IEmrRepository : IRepository<EmrRecord>
{
    /// <summary>
    /// Gets the comprehensive EMR for a patient, eager loading all related entities 
    /// (Allergies, MedicalHistory, Vitals).
    /// </summary>
    Task<EmrRecord?> GetByPatientIdWithDetailsAsync(Guid patientId, CancellationToken ct = default);
}
