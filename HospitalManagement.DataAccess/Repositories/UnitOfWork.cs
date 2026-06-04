using HospitalManagement.DataAccess.Context;
using HospitalManagement.DataAccess.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace HospitalManagement.DataAccess.Repositories;

/// <summary>
/// Unit of Work backed by EF Core. All repositories share a single DbContext instance
/// so they participate in the same transaction boundary.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    // Lazy-initialized typed repositories
    private IRepository<User>? _users;
    private IRepository<Patient>? _patients;
    private IRepository<Doctor>? _doctors;
    private IRepository<Appointment>? _appointments;
    private IVisitRepository? _visits;
    private IRepository<Prescription>? _prescriptions;
    private IRepository<LabReport>? _labReports;
    private IRepository<Billing>? _bills;
    private IRepository<Document>? _documents;
    private IRepository<InsuranceClaim>? _insuranceClaims;
    private IRepository<DoctorReview>? _doctorReviews;
    private IRepository<SystemSetting>? _systemSettings;
    private IEmrRepository? _emrRecords;
    private IRepository<QueueEntry>? _queueEntries;
    private IRepository<DoctorSchedule>? _doctorSchedules;
    private IRepository<DoctorLeave>? _doctorLeaves;
    private IRepository<BlockedSlot>? _blockedSlots;
    private IRepository<Department>? _departments;
    private IRepository<PatientConsent>? _patientConsents;
    private IRepository<Ward>? _wards;
    private IRepository<Bed>? _beds;
    private IRepository<AdmissionRecord>? _admissionRecords;
    private IRepository<MedicationInventory>? _medicationInventories;
    private IRepository<DispensationRecord>? _dispensationRecords;
    private IRepository<DispensedItem>? _dispensedItems;
    private IRepository<ChatMessage>? _chatMessages;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<User> Users
        => _users ??= new GenericRepository<User>(_context);

    public IRepository<Patient> Patients
        => _patients ??= new GenericRepository<Patient>(_context);

    public IRepository<Doctor> Doctors
        => _doctors ??= new GenericRepository<Doctor>(_context);

    public IRepository<Appointment> Appointments => _appointments ??= new GenericRepository<Appointment>(_context);
    public IVisitRepository Visits => _visits ??= new VisitRepository(_context);
    public IRepository<Prescription> Prescriptions => _prescriptions ??= new GenericRepository<Prescription>(_context);

    public IRepository<LabReport> LabReports
        => _labReports ??= new GenericRepository<LabReport>(_context);

    public IRepository<Billing> Bills
        => _bills ??= new GenericRepository<Billing>(_context);

    public IRepository<Document> Documents 
        => _documents ??= new GenericRepository<Document>(_context);

    public IRepository<InsuranceClaim> InsuranceClaims 
        => _insuranceClaims ??= new GenericRepository<InsuranceClaim>(_context);

    public IRepository<DoctorReview> DoctorReviews 
        => _doctorReviews ??= new GenericRepository<DoctorReview>(_context);

    public IRepository<SystemSetting> SystemSettings 
        => _systemSettings ??= new GenericRepository<SystemSetting>(_context);

    public IEmrRepository EmrRecords
        => _emrRecords ??= new EmrRepository(_context);

    // Phase 4
    public IRepository<QueueEntry> QueueEntries => _queueEntries ??= new GenericRepository<QueueEntry>(_context);
    
    private IRepository<RefreshToken>? _refreshTokens;
    public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new GenericRepository<RefreshToken>(_context);
    
    private IRepository<Consultation>? _consultations;
    public IRepository<Consultation> Consultations => _consultations ??= new GenericRepository<Consultation>(_context);
    
    private IRepository<Notification>? _notifications;
    public IRepository<Notification> Notifications => _notifications ??= new GenericRepository<Notification>(_context);
    public IRepository<DoctorSchedule> DoctorSchedules 
        => _doctorSchedules ??= new GenericRepository<DoctorSchedule>(_context);
    public IRepository<DoctorLeave> DoctorLeaves 
        => _doctorLeaves ??= new GenericRepository<DoctorLeave>(_context);
    public IRepository<BlockedSlot> BlockedSlots 
        => _blockedSlots ??= new GenericRepository<BlockedSlot>(_context);
    public IRepository<Department> Departments 
        => _departments ??= new GenericRepository<Department>(_context);
    public IRepository<PatientConsent> PatientConsents 
        => _patientConsents ??= new GenericRepository<PatientConsent>(_context);

    // IPD
    public IRepository<Ward> Wards => _wards ??= new GenericRepository<Ward>(_context);
    public IRepository<Bed> Beds => _beds ??= new GenericRepository<Bed>(_context);
    public IRepository<AdmissionRecord> AdmissionRecords => _admissionRecords ??= new GenericRepository<AdmissionRecord>(_context);

    // Pharmacy
    public IRepository<MedicationInventory> MedicationInventories => _medicationInventories ??= new GenericRepository<MedicationInventory>(_context);
    public IRepository<DispensationRecord> DispensationRecords => _dispensationRecords ??= new GenericRepository<DispensationRecord>(_context);
    public IRepository<DispensedItem> DispensedItems => _dispensedItems ??= new GenericRepository<DispensedItem>(_context);

    // Chat
    public IRepository<ChatMessage> ChatMessages => _chatMessages ??= new GenericRepository<ChatMessage>(_context);

    /// <inheritdoc/>
    public async Task<int> CompleteAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    /// <inheritdoc/>
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction != null) return;
        _currentTransaction = await _context.Database.BeginTransactionAsync(ct);
    }

    /// <inheritdoc/>
    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction == null)
            throw new InvalidOperationException("No active transaction to commit.");

        await _context.SaveChangesAsync(ct);
        await _currentTransaction.CommitAsync(ct);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    /// <inheritdoc/>
    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction == null) return;
        await _currentTransaction.RollbackAsync(ct);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}
