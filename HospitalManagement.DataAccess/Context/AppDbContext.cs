using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Models.Emr;
using HospitalManagement.DataAccess.Models.Billing;
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
    public DbSet<VisitHistory> VisitHistories => Set<VisitHistory>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<LabReport> LabReports => Set<LabReport>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<BillingAudit> BillingAudits => Set<BillingAudit>();
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
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();

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
    public DbSet<Immunization> Immunizations => Set<Immunization>();
    public DbSet<EmrDocument> EmrDocuments => Set<EmrDocument>();

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
        modelBuilder.Entity<VisitHistory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Prescription>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PrescriptionItem>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LabReport>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<InvoiceItem>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Payment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Refund>().HasQueryFilter(e => !e.IsDeleted);
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
        modelBuilder.Entity<Immunization>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<EmrDocument>().HasQueryFilter(e => !e.IsDeleted);

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

            e.HasIndex(a => new { a.PatientId, a.AppointmentTime });
            e.HasIndex(a => a.Status);
            e.HasIndex(a => a.QueueNumber);

            e.Property(a => a.Status).HasConversion<string>();
            e.Property(a => a.Type).HasConversion<string>();
            e.Property(a => a.Priority).HasConversion<string>();
            e.Property(a => a.Source).HasConversion<string>();
            e.Property(a => a.Reason).HasMaxLength(500);
            e.Property(a => a.MeetingUrl).HasMaxLength(1000);
        });

        // ── Visit ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Visit>(e =>
        {
            e.HasIndex(v => v.VisitNumber).IsUnique();
            e.HasIndex(v => v.PatientId);
            e.HasIndex(v => v.DoctorId);
            e.HasIndex(v => v.AppointmentId);
            e.HasIndex(v => v.Status);
            e.HasIndex(v => v.VisitType);
            e.HasIndex(v => v.CheckInTime);

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

            e.HasOne(v => v.Department)
             .WithMany()
             .HasForeignKey(v => v.DepartmentId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(v => v.Consultation)
             .WithOne(c => c.Visit)
             .HasForeignKey<Consultation>(c => c.VisitId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            // Removing the 1:1 constraint on Billing here as Visit will map to Invoice via Invoice.VisitId 
            // and we will configure Invoice below.

            e.Property(v => v.VisitNumber).HasMaxLength(50).IsRequired();
            e.Property(v => v.Status).HasConversion<string>();
            e.Property(v => v.VisitType).HasConversion<string>();
        });

        // ── Consultation ──────────────────────────────────────────────────────
        modelBuilder.Entity<Consultation>(e =>
        {
            e.HasOne(c => c.Doctor)
             .WithMany()
             .HasForeignKey(c => c.DoctorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(c => c.Status).HasConversion<string>();

            e.HasIndex(c => c.VisitId).IsUnique();
            e.HasIndex(c => c.DoctorId);
            e.HasIndex(c => c.Status);
            e.HasIndex(c => c.CreatedAt);
            e.HasIndex(c => c.FollowUpDate);
        });

        // ── VisitHistory ──────────────────────────────────────────────────────
        modelBuilder.Entity<VisitHistory>(e =>
        {
            e.HasOne(h => h.Visit)
             .WithMany()
             .HasForeignKey(h => h.VisitId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(h => h.PreviousState).HasConversion<string>();
            e.Property(h => h.NewState).HasConversion<string>();
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
            e.HasOne(p => p.Consultation)
             .WithMany(c => c.Prescriptions)
             .HasForeignKey(p => p.ConsultationId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Doctor)
             .WithMany(d => d.Prescriptions)
             .HasForeignKey(p => p.DoctorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Patient)
             .WithMany(pt => pt.Prescriptions)
             .HasForeignKey(p => p.PatientId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(p => p.Status).HasConversion<string>();
        });

        // ── PrescriptionItem ──────────────────────────────────────────────────
        modelBuilder.Entity<PrescriptionItem>(e =>
        {
            e.HasOne(pi => pi.Prescription)
             .WithMany(p => p.Items)
             .HasForeignKey(pi => pi.PrescriptionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(pi => pi.MedicationName).HasMaxLength(200).IsRequired();
            e.Property(pi => pi.Strength).HasMaxLength(100);
            e.Property(pi => pi.Dosage).HasMaxLength(100).IsRequired();
            e.Property(pi => pi.Frequency).HasMaxLength(100).IsRequired();
        });

        // ── LabReport ─────────────────────────────────────────────────────────
        modelBuilder.Entity<LabReport>(e =>
        {
            e.HasOne(lr => lr.Patient)
             .WithMany(p => p.LabReports)
             .HasForeignKey(lr => lr.PatientId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(lr => lr.Consultation)
             .WithMany(c => c.LabReports)
             .HasForeignKey(lr => lr.ConsultationId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(lr => lr.Doctor)
             .WithMany()
             .HasForeignKey(lr => lr.DoctorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(lr => lr.Status).HasConversion<string>();
            e.Property(lr => lr.ReportName).HasMaxLength(300).IsRequired();
        });

        // ── Invoice ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Invoice>(e =>
        {
            e.HasIndex(i => i.InvoiceNumber).IsUnique();
            e.HasIndex(i => i.VisitId);

            e.HasOne(i => i.Visit)
             .WithMany()
             .HasForeignKey(i => i.VisitId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(i => i.Patient)
             .WithMany() // or .WithMany(p => p.Invoices) if added to Patient
             .HasForeignKey(i => i.PatientId)
             .OnDelete(DeleteBehavior.Restrict);
             
            e.HasOne(i => i.Doctor)
             .WithMany()
             .HasForeignKey(i => i.DoctorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(i => i.Status).HasConversion<string>();
            e.Property(i => i.Subtotal).HasColumnType("decimal(18,2)");
            e.Property(i => i.DiscountAmount).HasColumnType("decimal(18,2)");
            e.Property(i => i.TaxAmount).HasColumnType("decimal(18,2)");
            e.Property(i => i.InsuranceCoverage).HasColumnType("decimal(18,2)");
            e.Property(i => i.PatientResponsibility).HasColumnType("decimal(18,2)");
            e.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
        });

        // ── InvoiceItem ────────────────────────────────────────────────────────
        modelBuilder.Entity<InvoiceItem>(e =>
        {
            e.HasOne(ii => ii.Invoice)
             .WithMany(i => i.Items)
             .HasForeignKey(ii => ii.InvoiceId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(ii => ii.ItemType).HasConversion<string>();
            e.Property(ii => ii.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(ii => ii.Amount).HasColumnType("decimal(18,2)");
        });

        // ── Payment ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Payment>(e =>
        {
            e.HasOne(p => p.Invoice)
             .WithMany(i => i.Payments)
             .HasForeignKey(p => p.InvoiceId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(p => p.PaymentMethod).HasConversion<string>();
            e.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        });

        // ── Refund ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Refund>(e =>
        {
            e.HasOne(r => r.Invoice)
             .WithMany()
             .HasForeignKey(r => r.InvoiceId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Payment)
             .WithMany()
             .HasForeignKey(r => r.PaymentId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(r => r.Amount).HasColumnType("decimal(18,2)");
        });

        // ── BillingAudit ──────────────────────────────────────────────────────
        modelBuilder.Entity<BillingAudit>(e =>
        {
            e.HasIndex(ba => ba.EntityId);
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

        // ── Immunization ──────────────────────────────────────────────────────
        modelBuilder.Entity<Immunization>(e =>
        {
            e.HasOne(i => i.EmrRecord)
             .WithMany(emr => emr.Immunizations)
             .HasForeignKey(i => i.EmrRecordId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(i => i.VaccineName).HasMaxLength(200).IsRequired();
            e.Property(i => i.DoseNumber).HasMaxLength(50);
            e.Property(i => i.Provider).HasMaxLength(200);
        });

        // ── EmrDocument ───────────────────────────────────────────────────────
        modelBuilder.Entity<EmrDocument>(e =>
        {
            e.HasOne(d => d.EmrRecord)
             .WithMany(emr => emr.Documents)
             .HasForeignKey(d => d.EmrRecordId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(d => d.BlobUrl).HasMaxLength(1000).IsRequired();
            e.Property(d => d.FileName).HasMaxLength(300).IsRequired();
            e.Property(d => d.ContentType).HasMaxLength(100).IsRequired();
        });

        // ── InsuranceClaim ────────────────────────────────────────────────────
        modelBuilder.Entity<InsuranceClaim>(e =>
        {
            e.HasOne(ic => ic.Invoice)
             .WithMany(i => i.InsuranceClaims)
             .HasForeignKey(ic => ic.InvoiceId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(ic => ic.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.Property(ic => ic.RequestedAmount).HasColumnType("decimal(18,2)");
            e.Property(ic => ic.ApprovedAmount).HasColumnType("decimal(18,2)");
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

        // ── LoginHistory ──────────────────────────────────────────────────────
        modelBuilder.Entity<LoginHistory>(e =>
        {
            e.HasOne(lh => lh.User)
             .WithMany() // or WithMany(u => u.LoginHistories) if navigation property added
             .HasForeignKey(lh => lh.UserId)
             .OnDelete(DeleteBehavior.Cascade);
             
            e.HasIndex(lh => lh.UserId);
            e.HasIndex(lh => lh.Timestamp).IsDescending();
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
