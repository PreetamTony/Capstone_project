using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Models.Emr;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.DataAccess.Context;

/// <summary>
/// Main EF Core DbContext. Configures all entity relationships, constraints, and indexes.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientConsent> PatientConsents => Set<PatientConsent>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<LabReport> LabReports => Set<LabReport>();
    public DbSet<Billing> Billings => Set<Billing>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<InsuranceClaim> InsuranceClaims => Set<InsuranceClaim>();
    public DbSet<DoctorReview> DoctorReviews => Set<DoctorReview>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    // IPD
    public DbSet<Ward> Wards => Set<Ward>();
    public DbSet<Bed> Beds => Set<Bed>();
    public DbSet<AdmissionRecord> AdmissionRecords => Set<AdmissionRecord>();

    // Pharmacy
    public DbSet<MedicationInventory> MedicationInventories => Set<MedicationInventory>();
    public DbSet<DispensationRecord> DispensationRecords => Set<DispensationRecord>();
    public DbSet<DispensedItem> DispensedItems => Set<DispensedItem>();

    // Chat
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Consultation> Consultations { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;

    // Phase 4
    public DbSet<QueueEntry> QueueEntries => Set<QueueEntry>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
    public DbSet<DoctorLeave> DoctorLeaves => Set<DoctorLeave>();
    public DbSet<BlockedSlot> BlockedSlots => Set<BlockedSlot>();

    // EMR Data
    public DbSet<EmrRecord> EmrRecords => Set<EmrRecord>();
    public DbSet<Allergy> Allergies => Set<Allergy>();
    public DbSet<MedicalHistory> MedicalHistories => Set<MedicalHistory>();
    public DbSet<Vitals> Vitals => Set<Vitals>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Global query filter: soft delete ─────────────────────────────────
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Patient>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PatientConsent>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Department>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Doctor>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Appointment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Visit>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Prescription>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LabReport>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Billing>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Document>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<InsuranceClaim>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DoctorReview>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SystemSetting>().HasQueryFilter(e => !e.IsDeleted);
        
        modelBuilder.Entity<Ward>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Bed>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AdmissionRecord>().HasQueryFilter(e => !e.IsDeleted);
        
        modelBuilder.Entity<MedicationInventory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DispensationRecord>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DispensedItem>().HasQueryFilter(e => !e.IsDeleted);
        
        modelBuilder.Entity<ChatMessage>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<QueueEntry>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<RefreshToken>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Consultation>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Notification>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<DoctorSchedule>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DoctorLeave>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<BlockedSlot>().HasQueryFilter(e => !e.IsDeleted);

        // EMR Soft Delete
        modelBuilder.Entity<EmrRecord>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Allergy>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<MedicalHistory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Vitals>().HasQueryFilter(e => !e.IsDeleted);

        // ── Chat ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<ChatMessage>(e =>
        {
            e.HasOne(cm => cm.Sender)
             .WithMany()
             .HasForeignKey(cm => cm.SenderId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(cm => cm.Receiver)
             .WithMany()
             .HasForeignKey(cm => cm.ReceiverId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Seed Admin User ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.PhoneNumber).HasMaxLength(20);
            e.Property(u => u.Role).HasConversion<string>();
        });

        // ── Patient ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Patient>(e =>
        {
            e.HasOne(p => p.User)
             .WithOne(u => u.Patient)
             .HasForeignKey<Patient>(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            e.Property(p => p.LastName).HasMaxLength(100).IsRequired();
            e.Property(p => p.InsuranceCoveragePercent).HasColumnType("decimal(5,2)");
            e.Property(p => p.Gender).HasConversion<string>();
            e.Property(p => p.BloodGroup).HasConversion<string>();
            e.Ignore(p => p.Age);
            e.Ignore(p => p.FullName);
        });

        // ── Doctor ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Doctor>(e =>
        {
            e.HasOne(d => d.User)
             .WithOne(u => u.Doctor)
             .HasForeignKey<Doctor>(d => d.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(d => d.LicenseNumber).IsUnique();
            e.Property(d => d.FirstName).HasMaxLength(100).IsRequired();
            e.Property(d => d.LastName).HasMaxLength(100).IsRequired();
            e.Property(d => d.Specialization).HasMaxLength(200).IsRequired();
            e.Property(d => d.LicenseNumber).HasMaxLength(50).IsRequired();
            e.Property(d => d.ConsultationFee).HasColumnType("decimal(10,2)");
            e.Property(d => d.Rating).HasColumnType("decimal(3,2)");
            e.Ignore(d => d.FullName);
        });

        // ── Department ────────────────────────────────────────────────────────
        modelBuilder.Entity<Department>(e =>
        {
            e.HasMany(d => d.Doctors)
             .WithOne(doc => doc.Department)
             .HasForeignKey(doc => doc.DepartmentId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Appointment ───────────────────────────────────────────────────────
        modelBuilder.Entity<Appointment>(e =>
        {
            // Unique: one slot per doctor per time
            e.HasIndex(a => new { a.DoctorId, a.AppointmentTime })
             .IsUnique()
             .HasFilter("\"IsDeleted\" = false");

            e.HasOne(a => a.Patient)
             .WithMany(p => p.Appointments)
             .HasForeignKey(a => a.PatientId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Doctor)
             .WithMany(d => d.Appointments)
             .HasForeignKey(a => a.DoctorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(a => a.Status).HasConversion<string>();
            e.Property(a => a.Type).HasConversion<string>();
            e.Property(a => a.Priority).HasConversion<string>();
            e.Property(a => a.Reason).HasMaxLength(500);
        });

        // ── Visit ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Visit>(e =>
        {
            e.HasOne(v => v.Patient)
             .WithMany(p => p.Visits)
             .HasForeignKey(v => v.PatientId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(v => v.Doctor)
             .WithMany(d => d.Visits)
             .HasForeignKey(v => v.DoctorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(v => v.Appointment)
             .WithOne(a => a.Visit)
             .HasForeignKey<Visit>(v => v.AppointmentId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);

            e.Property(v => v.Status).HasConversion<string>();
        });

        // ── QueueEntry ────────────────────────────────────────────────────────
        modelBuilder.Entity<QueueEntry>(e =>
        {
            e.HasOne(q => q.Patient)
             .WithMany()
             .HasForeignKey(q => q.PatientId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(q => q.Doctor)
             .WithMany()
             .HasForeignKey(q => q.DoctorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(q => q.Visit)
             .WithMany()
             .HasForeignKey(q => q.VisitId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(q => q.Status).HasConversion<string>();
        });

        // ── DoctorSchedule ────────────────────────────────────────────────────
        modelBuilder.Entity<DoctorSchedule>(e =>
        {
            e.HasOne(s => s.Doctor)
             .WithMany()
             .HasForeignKey(s => s.DoctorId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── DoctorLeave ───────────────────────────────────────────────────────
        modelBuilder.Entity<DoctorLeave>(e =>
        {
            e.HasOne(l => l.Doctor)
             .WithMany()
             .HasForeignKey(l => l.DoctorId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── BlockedSlot ───────────────────────────────────────────────────────
        modelBuilder.Entity<BlockedSlot>(e =>
        {
            e.HasOne(b => b.Doctor)
             .WithMany()
             .HasForeignKey(b => b.DoctorId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Prescription ──────────────────────────────────────────────────────
        modelBuilder.Entity<Prescription>(e =>
        {
            e.HasOne(p => p.Visit)
             .WithMany(v => v.Prescriptions)
             .HasForeignKey(p => p.VisitId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Doctor)
             .WithMany(d => d.Prescriptions)
             .HasForeignKey(p => p.DoctorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Patient)
             .WithMany(pt => pt.Prescriptions)
             .HasForeignKey(p => p.PatientId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(p => p.MedicationName).HasMaxLength(200).IsRequired();
            e.Property(p => p.Dosage).HasMaxLength(100).IsRequired();
            e.Property(p => p.Frequency).HasMaxLength(100).IsRequired();
            e.Ignore(p => p.IsEditable);
        });

        // ── LabReport ─────────────────────────────────────────────────────────
        modelBuilder.Entity<LabReport>(e =>
        {
            e.HasOne(lr => lr.Patient)
             .WithMany(p => p.LabReports)
             .HasForeignKey(lr => lr.PatientId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(lr => lr.Visit)
             .WithMany(v => v.LabReports)
             .HasForeignKey(lr => lr.VisitId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(lr => lr.Doctor)
             .WithMany()
             .HasForeignKey(lr => lr.DoctorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(lr => lr.Status).HasConversion<string>();
            e.Property(lr => lr.ReportName).HasMaxLength(300).IsRequired();
        });

        // ── Billing ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Billing>(e =>
        {
            e.HasIndex(b => b.VisitId).IsUnique();

            e.HasOne(b => b.Visit)
             .WithOne(v => v.Billing)
             .HasForeignKey<Billing>(b => b.VisitId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(b => b.Patient)
             .WithMany(p => p.Bills)
             .HasForeignKey(b => b.PatientId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(b => b.Amount).HasColumnType("decimal(10,2)");
            e.Property(b => b.InsuranceCoverage).HasColumnType("decimal(10,2)");
            e.Property(b => b.PatientResponsibility).HasColumnType("decimal(10,2)");
            e.Property(b => b.Status).HasConversion<string>();
        });

        // ── EMR Record ────────────────────────────────────────────────────────
        modelBuilder.Entity<EmrRecord>(e =>
        {
            e.HasOne(emr => emr.Patient)
             .WithOne(p => p.EmrRecord)
             .HasForeignKey<EmrRecord>(emr => emr.PatientId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Allergy ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Allergy>(e =>
        {
            e.HasOne(a => a.EmrRecord)
             .WithMany(emr => emr.Allergies)
             .HasForeignKey(a => a.EmrRecordId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(a => a.Substance).HasMaxLength(200).IsRequired();
            e.Property(a => a.Severity).HasMaxLength(50);
            e.Property(a => a.Reaction).HasMaxLength(500);
        });

        // ── MedicalHistory ────────────────────────────────────────────────────
        modelBuilder.Entity<MedicalHistory>(e =>
        {
            e.HasOne(mh => mh.EmrRecord)
             .WithMany(emr => emr.MedicalHistories)
             .HasForeignKey(mh => mh.EmrRecordId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(mh => mh.Condition).HasMaxLength(200).IsRequired();
            e.Property(mh => mh.Status).HasMaxLength(50);
        });

        // ── Vitals ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Vitals>(e =>
        {
            e.HasOne(v => v.EmrRecord)
             .WithMany(emr => emr.Vitals)
             .HasForeignKey(v => v.EmrRecordId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(v => v.Visit)
             .WithMany(vi => vi.Vitals)
             .HasForeignKey(v => v.VisitId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);

            e.Property(v => v.Temperature).HasColumnType("decimal(5,2)");
            e.Property(v => v.O2Saturation).HasColumnType("decimal(5,2)");
            e.Property(v => v.Height).HasColumnType("decimal(5,2)");
            e.Property(v => v.Weight).HasColumnType("decimal(5,2)");
        });

        // ── AuditLog ──────────────────────────────────────────────────────────
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(al => al.Id);
            e.Property(al => al.Id).ValueGeneratedOnAdd();
            e.Property(al => al.Action).HasConversion<string>().HasMaxLength(50).IsRequired();
            e.Property(al => al.EntityName).HasMaxLength(100).IsRequired();
            e.Property(al => al.RecordId).HasMaxLength(100).IsRequired();
            e.Property(al => al.IpAddress).HasMaxLength(45);
            e.Property(al => al.OldValues).HasColumnType("jsonb");
            e.Property(al => al.NewValues).HasColumnType("jsonb");
            e.Property(al => al.ChangedFields).HasColumnType("jsonb");

            // Indexes for faster search
            e.HasIndex(al => al.Timestamp).IsDescending();
            e.HasIndex(al => al.UserId);
            e.HasIndex(al => al.EntityName);
            e.HasIndex(al => al.Action);
            e.HasIndex(al => al.CorrelationId);
        });
    }

    /// <summary>
    /// Intercept SaveChanges to auto-update UpdatedAt timestamp.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
