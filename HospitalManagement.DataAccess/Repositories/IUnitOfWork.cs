using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Emr;
using HospitalManagement.DataAccess.Models.Billing;

namespace HospitalManagement.DataAccess.Repositories;

/// <summary>
/// Unit of Work — coordinates transactions and exposes typed repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IVisitRepository Visits { get; }
    
    // Phase 4
    IRepository<QueueEntry> QueueEntries { get; }
    IRepository<RefreshToken> RefreshTokens { get; }
    IRepository<Consultation> Consultations { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<DoctorSchedule> DoctorSchedules { get; }
    IRepository<DoctorLeave> DoctorLeaves { get; }
    IRepository<BlockedSlot> BlockedSlots { get; }
    IRepository<Department> Departments { get; }
    IRepository<Patient> Patients { get; }
    IRepository<PatientConsent> PatientConsents { get; }
    IRepository<Prescription> Prescriptions { get; }
    IRepository<PrescriptionItem> PrescriptionItems { get; }
    IRepository<LabReport> LabReports { get; }
    IRepository<Invoice> Invoices { get; }
    IRepository<InvoiceItem> InvoiceItems { get; }
    IRepository<Payment> Payments { get; }
    IRepository<Refund> Refunds { get; }
    IRepository<BillingAudit> BillingAudits { get; }
    IRepository<Document> Documents { get; }
    IRepository<InsuranceClaim> InsuranceClaims { get; }
    IRepository<DoctorReview> DoctorReviews { get; }
    IRepository<Doctor> Doctors { get; }
    IRepository<Appointment> Appointments { get; }
    IRepository<SystemSetting> SystemSettings { get; }
    
    // IPD
    IRepository<Ward> Wards { get; }
    IRepository<Bed> Beds { get; }
    IRepository<AdmissionRecord> AdmissionRecords { get; }

    // Pharmacy
    IRepository<MedicationInventory> MedicationInventories { get; }
    IRepository<DispensationRecord> DispensationRecords { get; }
    IRepository<DispensedItem> DispensedItems { get; }

    // Chat
    IRepository<ChatMessage> ChatMessages { get; }
    IQueryable<AuditLog> AuditLogs { get; }

    IRepository<VisitHistory> VisitHistories { get; }

    // Auth
    IRepository<LoginHistory> LoginHistories { get; }

    IEmrRepository EmrRecords { get; }
    IRepository<Allergy> Allergies { get; }
    IRepository<MedicalHistory> MedicalHistories { get; }
    IRepository<Vitals> Vitals { get; }
    IRepository<Immunization> Immunizations { get; }
    IRepository<EmrDocument> EmrDocuments { get; }

    /// <summary>Persist all pending changes in a single transaction.</summary>
    Task<int> CompleteAsync(CancellationToken ct = default);

    /// <summary>Begin an explicit database transaction.</summary>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>Commit the active transaction.</summary>
    Task CommitTransactionAsync(CancellationToken ct = default);

    /// <summary>Roll back the active transaction.</summary>
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
