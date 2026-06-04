using HospitalManagement.BusinessLogic.DTOs.Admin;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Context;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Repositories;
using HospitalManagement.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.BusinessLogic.Services;

public partial class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<AdminService> _logger;
    private readonly ICurrentUserService _currentUserService;

    public AdminService(AppDbContext context, IUnitOfWork uow, ILogger<AdminService> logger, ICurrentUserService currentUserService)
    {
        _context = context;
        _uow = uow;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<AuditLogResponseDto>> GetAuditLogsAsync(
        Guid? userId, string? entityName, string? action, 
        DateTime? fromDate, DateTime? toDate, string? search, 
        int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving audit logs page {PageNumber}", pageNumber);
        if (pageSize > 100)
        {
            _logger.LogWarning("Audit logs pagination violation: requested {PageSize}", pageSize);
            throw new FluentValidation.ValidationException(new[] { 
                new FluentValidation.Results.ValidationFailure("PageSize", "Maximum page size is 100.") 
            });
        }

        var query = _context.AuditLogs.AsNoTracking().Where(a => !a.IsArchived);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(a => a.EntityName == entityName);

        if (!string.IsNullOrWhiteSpace(action) && Enum.TryParse<AuditActionType>(action, true, out var actionEnum))
            query = query.Where(a => a.Action == actionEnum);
            
        if (fromDate.HasValue)
            query = query.Where(a => a.Timestamp >= fromDate.Value);
            
        if (toDate.HasValue)
            query = query.Where(a => a.Timestamp <= toDate.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(a => 
                (a.UserEmail != null && a.UserEmail.ToLower().Contains(searchLower)) ||
                (a.EntityName != null && a.EntityName.ToLower().Contains(searchLower)) ||
                (a.RecordId != null && a.RecordId.ToLower().Contains(searchLower))
            );
        }

        var total = await query.CountAsync(ct);
        
        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogResponseDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserEmail = a.UserEmail,
                UserRole = a.UserRole,
                PerformedByName = a.PerformedByName,
                Action = a.Action.ToString(),
                EntityName = a.EntityName,
                RecordId = a.RecordId,
                ChangedFields = a.ChangedFields,
                CorrelationId = a.CorrelationId,
                Timestamp = a.Timestamp
            })
            .ToListAsync(ct);

        if (logs.Count == 0 && total == 0 && !string.IsNullOrWhiteSpace(search))
            _logger.LogInformation("Failed search for audit logs: {Search}", search);

        return PagedResult<AuditLogResponseDto>.Create(logs, total, pageNumber, pageSize);
    }

    public async Task<AuditLogDetailResponseDto> GetAuditLogByIdAsync(long id, CancellationToken ct = default)
    {
        var log = await _context.AuditLogs.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException("AuditLog", id);

        var changes = new Dictionary<string, ChangeDetailDto>();

        if (log.ChangedFields != null && log.ChangedFields.Any())
        {
            var oldDict = string.IsNullOrEmpty(log.OldValues) ? new Dictionary<string, object?>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(log.OldValues) ?? new Dictionary<string, object?>();
            
            var newDict = string.IsNullOrEmpty(log.NewValues) ? new Dictionary<string, object?>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(log.NewValues) ?? new Dictionary<string, object?>();

            foreach (var field in log.ChangedFields)
            {
                oldDict.TryGetValue(field, out var oldVal);
                newDict.TryGetValue(field, out var newVal);
                
                changes[field] = new ChangeDetailDto
                {
                    Old = oldVal,
                    New = newVal
                };
            }
        }

        return new AuditLogDetailResponseDto
        {
            Id = log.Id,
            UserId = log.UserId,
            UserEmail = log.UserEmail,
            UserRole = log.UserRole,
            PerformedByName = log.PerformedByName,
            Action = log.Action.ToString(),
            EntityName = log.EntityName,
            RecordId = log.RecordId,
            ChangedFields = log.ChangedFields,
            CorrelationId = log.CorrelationId,
            Timestamp = log.Timestamp,
            Changes = changes.Count > 0 ? changes : null,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent
        };
    }

    public async Task<DailySummaryDto> GetDailySummaryAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var targetDate = (fromDate ?? DateTime.UtcNow).Date;
        var endTargetDate = (toDate ?? targetDate).Date.AddDays(1).AddTicks(-1);

        // Previous Day range
        var previousDayStart = targetDate.AddDays(-1);
        var previousDayEnd = endTargetDate.AddDays(-1);

        // 1. Appointments
        var appointments = await _uow.Appointments.Query()
            .Include(a => a.Doctor)
            .ThenInclude(d => d.Department)
            .Where(a => a.AppointmentTime >= targetDate && a.AppointmentTime <= endTargetDate)
            .ToListAsync(ct);

        var prevAppointments = await _uow.Appointments.Query()
            .Where(a => a.AppointmentTime >= previousDayStart && a.AppointmentTime <= previousDayEnd)
            .CountAsync(ct);

        var totalAppointments = appointments.Count;
        var completedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed);
        var cancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);
        var noShows = appointments.Count(a => a.Status == AppointmentStatus.NoShow);

        // 2. Patient Metrics
        var newPatients = await _uow.Patients
            .CountAsync(p => p.CreatedAt >= targetDate && p.CreatedAt <= endTargetDate, ct);
            
        var uniquePatientIds = appointments.Select(a => a.PatientId).Distinct().ToList();
        var totalPatientsServed = uniquePatientIds.Count;
        var returningPatients = totalPatientsServed - newPatients; // Approximation
        if (returningPatients < 0) returningPatients = 0;

        var teleconsultations = appointments.Count(a => a.Type == AppointmentType.Video || a.Type == AppointmentType.Phone);
        var walkInPatients = totalAppointments - teleconsultations; // Assuming remaining are walk-in/in-person

        // 3. Financial Metrics
        var billings = await _context.Billings
            .Where(b => b.CreatedAt >= targetDate && b.CreatedAt <= endTargetDate)
            .ToListAsync(ct);

        var prevBillings = await _context.Billings
            .Where(b => b.CreatedAt >= previousDayStart && b.CreatedAt <= previousDayEnd && b.Status == BillingStatus.Paid)
            .SumAsync(b => b.Amount, ct);

        var totalRevenue = billings.Where(b => b.Status == BillingStatus.Paid).Sum(b => b.Amount);
        var consultationRevenue = billings.Where(b => b.Status == BillingStatus.Paid && b.Category == BillingCategory.Consultation).Sum(b => b.Amount);
        var labRevenue = billings.Where(b => b.Status == BillingStatus.Paid && b.Category == BillingCategory.Lab).Sum(b => b.Amount);
        var pharmacyRevenue = billings.Where(b => b.Status == BillingStatus.Paid && b.Category == BillingCategory.Pharmacy).Sum(b => b.Amount);
        
        var insuranceClaims = billings.Count(b => b.InsuranceCoverage > 0);
        var insuranceAmount = billings.Sum(b => b.InsuranceCoverage);
        var pendingPayments = billings.Where(b => b.Status == BillingStatus.Pending).Sum(b => b.Amount);
        var refundsProcessed = billings.Where(b => b.Status == BillingStatus.Refunded).Sum(b => b.Amount);

        // 4. Operational Metrics
        var isoDayOfWeek = targetDate.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)targetDate.DayOfWeek;
        var doctorsOnLeave = await _context.DoctorLeaves
            .Where(l => l.StartDateTime.Date <= targetDate && l.EndDateTime.Date >= targetDate && l.IsApproved)
            .Select(l => l.DoctorId)
            .ToListAsync(ct);

        var availableDoctors = await _context.DoctorSchedules
            .Where(s => s.DayOfWeek == isoDayOfWeek && !doctorsOnLeave.Contains(s.DoctorId))
            .ToListAsync(ct);

        var totalDoctorsAvailable = availableDoctors.Select(s => s.DoctorId).Distinct().Count();
        
        // Calculate max theoretical capacity (assuming 15 mins per slot)
        double totalCapacitySlots = 0;
        foreach (var schedule in availableDoctors)
        {
            var duration = schedule.EndTime - schedule.StartTime;
            totalCapacitySlots += duration.TotalMinutes / 15.0; // 15 min slots
        }
        
        var appointmentUtilizationRate = totalCapacitySlots > 0 ? (totalAppointments / totalCapacitySlots) * 100 : 0;
        if (appointmentUtilizationRate > 100) appointmentUtilizationRate = 100; // Cap at 100%

        var pendingBillsCount = billings.Count(b => b.Status == BillingStatus.Pending);
        var labReportsPending = await _context.LabReports.CountAsync(lr => lr.Status == LabReportStatus.Pending && lr.CreatedAt <= endTargetDate, ct);
        var admittedPatients = await _context.AdmissionRecords.CountAsync(ar => ar.Status == "Admitted", ct);

        // Average times (Mocked logic from queues or just approximated if missing)
        var averageWaitTimeMinutes = 12.5; // Todo: fetch from QueueEntry CheckIn -> ConsultationStart
        var averageConsultationTimeMinutes = 18.2;

        // 5. Doctor Utilization
        var doctorUtilization = appointments
            .Where(a => a.Doctor != null)
            .GroupBy(a => new { a.DoctorId, a.Doctor.FirstName, a.Doctor.LastName })
            .Select(g => new DoctorUtilizationDto
            {
                DoctorId = g.Key.DoctorId,
                DoctorName = $"Dr. {g.Key.FirstName} {g.Key.LastName}",
                PatientsSeen = g.Count(),
                AverageWaitTime = 10.0, // Mocked
                Revenue = billings.Where(b => b.VisitId != Guid.Empty && g.Select(a => a.Id).Contains(b.VisitId)).Sum(b => b.Amount) // Appx
            })
            .ToList();

        // 6. Department Metrics
        var departmentMetrics = appointments
            .Where(a => a.Doctor != null && a.Doctor.Department != null)
            .GroupBy(a => a.Doctor.Department.Name)
            .ToDictionary(g => g.Key, g => new DepartmentMetricsDto
            {
                Appointments = g.Count(),
                Revenue = billings.Where(b => b.VisitId != Guid.Empty && g.Select(a => a.Id).Contains(b.VisitId)).Sum(b => b.Amount),
                WaitTime = 12.0 // Mocked
            });

        // 7. Time Slots (Peak Hour)
        var hourlyGroups = appointments
            .GroupBy(a => a.AppointmentTime.Hour)
            .ToDictionary(g => g.Key, g => g.Count());

        var peakHourStr = "N/A";
        var slowestHourStr = "N/A";
        if (hourlyGroups.Any())
        {
            var peakHourInt = hourlyGroups.OrderByDescending(kv => kv.Value).First().Key;
            var slowHourInt = hourlyGroups.OrderBy(kv => kv.Value).First().Key;
            peakHourStr = $"{peakHourInt:D2}:00-{(peakHourInt+1):D2}:00";
            slowestHourStr = $"{slowHourInt:D2}:00-{(slowHourInt+1):D2}:00";
        }

        // 8. Vs Previous Day
        var apptChange = prevAppointments == 0 ? 0 : ((double)(totalAppointments - prevAppointments) / prevAppointments) * 100;
        var revChange = prevBillings == 0 ? 0 : (double)((totalRevenue - prevBillings) / prevBillings) * 100;

        return new DailySummaryDto
        {
            Date = targetDate,
            TotalAppointments = totalAppointments,
            CompletedAppointments = completedAppointments,
            CancelledAppointments = cancelledAppointments,
            NoShows = noShows,
            AppointmentUtilizationRate = appointmentUtilizationRate,
            NewPatients = newPatients,
            ReturningPatients = returningPatients,
            TotalPatientsServed = totalPatientsServed,
            WalkInPatients = walkInPatients,
            Teleconsultations = teleconsultations,
            TotalRevenue = totalRevenue,
            ConsultationRevenue = consultationRevenue,
            LabRevenue = labRevenue,
            PharmacyRevenue = pharmacyRevenue,
            InsuranceClaims = insuranceClaims,
            InsuranceAmount = insuranceAmount,
            PendingPayments = pendingPayments,
            RefundsProcessed = refundsProcessed,
            AverageWaitTimeMinutes = averageWaitTimeMinutes,
            AverageConsultationTimeMinutes = averageConsultationTimeMinutes,
            TotalDoctorsAvailable = totalDoctorsAvailable,
            PendingBills = pendingBillsCount,
            LabReportsPending = labReportsPending,
            AdmittedPatients = admittedPatients,
            DoctorUtilization = doctorUtilization,
            DepartmentMetrics = departmentMetrics,
            PeakHour = peakHourStr,
            SlowestHour = slowestHourStr,
            VsPreviousDay = new PreviousDayComparisonDto
            {
                AppointmentsChange = Math.Round(apptChange, 2),
                RevenueChange = Math.Round(revChange, 2)
            }
        };
    }

    public async Task<PagedResult<UserSummaryDto>> GetUsersAsync(string? role, bool? isActive, string? search, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = _uow.Users.Query();

        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, true, out var userRole))
            query = query.Where(u => u.Role == userRole);
            
        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);
            
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u => u.Email.ToLower().Contains(searchLower) || (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(searchLower)));
        }

        var total = await query.CountAsync(ct);
        var users = await query
            .OrderBy(u => u.Email)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserSummaryDto
            {
                Id = u.Id,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
                IsActive = u.IsActive,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<UserSummaryDto>.Create(users, total, pageNumber, pageSize);
    }

    public async Task<CreateUserResponseDto> CreateUserAsync(CreateUserRequestDto request, CancellationToken ct = default)
    {
        if (await _uow.Users.AnyAsync(u => u.Email == request.Email.ToLower().Trim(), ct))
            throw new BusinessRuleViolationException("DuplicateEmail", $"Email '{request.Email}' is already registered.");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            throw new BusinessRuleViolationException("InvalidRole", $"Role '{request.Role}' is invalid.");

        // Generate temporary password
        var tempPassword = GenerateTemporaryPassword();

        var user = new User
        {
            Email = request.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
            PhoneNumber = request.PhoneNumber ?? string.Empty,
            Role = role,
            IsActive = true
        };

        await _uow.Users.AddAsync(user, ct);
        await _uow.CompleteAsync(ct);

        return new CreateUserResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            TemporaryPassword = tempPassword
        };
    }

    public async Task UpdateUserAsync(Guid userId, UpdateUserRequestDto request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct) ?? throw new NotFoundException("User", userId);

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (await _uow.Users.AnyAsync(u => u.Email == request.Email.ToLower().Trim() && u.Id != userId, ct))
                throw new BusinessRuleViolationException("DuplicateEmail", $"Email '{request.Email}' is already registered.");
            user.Email = request.Email.ToLower().Trim();
        }

        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;
        
        _uow.Users.Update(user);
        await _uow.CompleteAsync(ct);
    }

    public async Task ArchiveUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct) ?? throw new NotFoundException("User", userId);

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedBy = _currentUserService.UserId;
        
        _uow.Users.Update(user);
        await _uow.CompleteAsync(ct);
    }

    public async Task UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequestDto request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct) ?? throw new NotFoundException("User", userId);

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            throw new BusinessRuleViolationException("InvalidRole", $"Role '{request.Role}' is invalid.");

        user.Role = role;
        _uow.Users.Update(user);
        await _uow.CompleteAsync(ct);
    }

    public async Task ActivateUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct) ?? throw new NotFoundException("User", userId);

        user.IsActive = true;
        _uow.Users.Update(user);
        await _uow.CompleteAsync(ct);
    }

    public async Task DeactivateUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct) ?? throw new NotFoundException("User", userId);

        user.IsActive = false;
        _uow.Users.Update(user);
        await _uow.CompleteAsync(ct);
    }

    private string GenerateTemporaryPassword()
    {
        // Generates a random 10-character password like: A1b2C3d4E5!
        return Guid.NewGuid().ToString("N").Substring(0, 8) + "Aa1!";
    }

    public async Task<Guid> CreateDoctorProfileAsync(CreateDoctorProfileRequestDto request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, ct) 
            ?? throw new NotFoundException("User", request.UserId);

        if (user.Role != UserRole.Doctor)
            throw new BusinessRuleViolationException("InvalidRole", "User does not have Doctor role.");

        if (await _uow.Doctors.AnyAsync(d => d.UserId == request.UserId, ct))
            throw new BusinessRuleViolationException("DuplicateProfile", "Doctor profile already exists for this user.");

        var doctor = new Doctor
        {
            UserId = request.UserId,
            DepartmentId = request.DepartmentId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Specialization = request.Specialization,
            Qualification = request.Qualification,
            ExperienceYears = request.ExperienceYears,
            ConsultationFee = request.ConsultationFee
        };

        await _uow.Doctors.AddAsync(doctor, ct);
        await _uow.CompleteAsync(ct);

        return doctor.Id;
    }
}
