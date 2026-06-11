using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.LabReport;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Interfaces;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.BusinessLogic.Services;

public class LabReportService : ILabReportService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<LabReportService> _logger;
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    // Ideally configured in appsettings.json
    private readonly string _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "lab-reports");

    public LabReportService(
        IUnitOfWork uow, 
        ILogger<LabReportService> logger, 
        INotificationService notificationService,
        ICurrentUserService currentUserService)
    {
        _uow = uow;
        _logger = logger;
        _notificationService = notificationService;
        _currentUserService = currentUserService;
        
        if (!Directory.Exists(_uploadDirectory))
        {
            Directory.CreateDirectory(_uploadDirectory);
        }
    }

    public async Task<LabReportResponseDto> CreateLabOrderAsync(Guid doctorUserId, CreateLabOrderRequestDto request, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId, ct)
            ?? throw new NotFoundException("Doctor profile not found.");

        var consultation = await _uow.Consultations.Query()
            .Include(c => c.Visit)
            .FirstOrDefaultAsync(c => c.Id == request.ConsultationId, ct)
            ?? throw new NotFoundException("Consultation", request.ConsultationId);

        if (consultation.DoctorId != doctor.Id)
            throw new HospitalManagement.DataAccess.Exceptions.UnauthorizedAccessException("Only the assigned doctor can order tests for this consultation.");

        var labOrder = new LabReport
        {
            OrderNumber = $"LAB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
            ConsultationId = consultation.Id,
            DoctorId = doctor.Id,
            PatientId = consultation.Visit!.PatientId,
            TestName = request.TestName,
            Priority = request.Priority,
            OrderNotes = request.Notes,
            Status = LabReportStatus.Ordered,
            IsConfidential = request.IsConfidential,
            IsDeleted = false
        };

        await _uow.LabReports.AddAsync(labOrder, ct);
        await _uow.CompleteAsync(ct);

        _logger.LogInformation("Lab Order {OrderNumber} created by Doctor {DoctorId}", labOrder.OrderNumber, doctor.Id);

        return MapToDto(labOrder, consultation.Visit.Patient, doctor);
    }

    public async Task<LabReportResponseDto> UpdateOrderStatusAsync(Guid orderId, UpdateLabReportStatusDto request, CancellationToken ct = default)
    {
        var report = await _uow.LabReports.Query()
            .Include(l => l.Patient)
            .Include(l => l.Doctor)
            .FirstOrDefaultAsync(l => l.Id == orderId && !l.IsDeleted, ct)
            ?? throw new NotFoundException("Lab Order", orderId);

        if (!Enum.TryParse<LabReportStatus>(request.Status, true, out var newStatus))
            throw new BusinessRuleViolationException("InvalidStatus", $"Status '{request.Status}' is not valid.");

        report.Status = newStatus;

        _uow.LabReports.Update(report);
        await _uow.CompleteAsync(ct);

        if (newStatus == LabReportStatus.Completed)
        {
            await _notificationService.NotifyReportUploadedAsync(report.PatientId, report.Id, ct);
        }

        return MapToDto(report, report.Patient, report.Doctor);
    }

    public async Task<LabReportResponseDto> UploadLabReportAsync(Guid reportId, Guid uploaderUserId, UploadLabReportRequestDto request, CancellationToken ct = default)
    {
        var report = await _uow.LabReports.Query()
            .Include(l => l.Patient)
            .Include(l => l.Doctor)
            .FirstOrDefaultAsync(l => l.Id == reportId && !l.IsDeleted, ct)
            ?? throw new NotFoundException("Lab Order", reportId);

        if (request.File == null || request.File.Length == 0)
            throw new BusinessRuleViolationException("InvalidFile", "A valid file must be provided.");

        var originalFileName = request.File.FileName;
        var extension = Path.GetExtension(originalFileName);
        var safeFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(_uploadDirectory, safeFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await request.File.CopyToAsync(stream, ct);
        }

        report.ReportName = request.ReportName;
        report.ReportType = request.ReportType;
        report.Observations = request.Observations;
        report.FilePath = safeFileName;
        report.OriginalFileName = originalFileName;
        report.FileSizeBytes = request.File.Length;
        report.UploadedBy = uploaderUserId;
        report.Status = LabReportStatus.Completed; // Automatically complete on upload

        _uow.LabReports.Update(report);
        await _uow.CompleteAsync(ct);

        await _notificationService.NotifyReportUploadedAsync(report.PatientId, report.Id, ct);

        return MapToDto(report, report.Patient, report.Doctor);
    }

    public async Task<LabReportResponseDto> ReviewLabReportAsync(Guid reportId, Guid reviewerUserId, ReviewLabReportDto request, CancellationToken ct = default)
    {
        var report = await _uow.LabReports.Query()
            .Include(l => l.Patient)
            .Include(l => l.Doctor)
            .FirstOrDefaultAsync(l => l.Id == reportId && !l.IsDeleted, ct)
            ?? throw new NotFoundException("Lab Report", reportId);

        if (report.Doctor.UserId != reviewerUserId)
            throw new HospitalManagement.DataAccess.Exceptions.UnauthorizedAccessException("Only the assigned doctor can review this lab report.");

        if (report.Status != LabReportStatus.Completed && report.Status != LabReportStatus.Reviewed)
            throw new BusinessRuleViolationException("InvalidStatus", "Cannot review a report that is not Completed.");

        report.ReviewNotes = request.Notes;
        report.Status = LabReportStatus.Reviewed;

        _uow.LabReports.Update(report);
        await _uow.CompleteAsync(ct);

        return MapToDto(report, report.Patient, report.Doctor);
    }

    public async Task<LabReportResponseDto> GetByIdAsync(Guid id, Guid currentUserId, CancellationToken ct = default)
    {
        var report = await _uow.LabReports.Query()
            .Include(l => l.Patient)
            .Include(l => l.Doctor)
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, ct)
            ?? throw new NotFoundException("LabReport", id);

        CheckConfidentiality(report, currentUserId);

        return MapToDto(report, report.Patient, report.Doctor);
    }

    public async Task<(byte[] fileBytes, string contentType, string fileName)> DownloadReportAsync(Guid id, Guid currentUserId, CancellationToken ct = default)
    {
        var report = await _uow.LabReports.Query()
            .Include(l => l.Patient)
            .Include(l => l.Doctor)
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, ct)
            ?? throw new NotFoundException("LabReport", id);

        CheckConfidentiality(report, currentUserId);

        if (string.IsNullOrEmpty(report.FilePath))
            throw new BusinessRuleViolationException("FileNotFound", "No file is attached to this report.");

        var fullPath = Path.Combine(_uploadDirectory, report.FilePath);

        if (!File.Exists(fullPath))
            throw new NotFoundException("File", report.FilePath);

        var bytes = await File.ReadAllBytesAsync(fullPath, ct);
        
        var ext = Path.GetExtension(report.OriginalFileName ?? "").ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

        return (bytes, contentType, report.OriginalFileName ?? report.FilePath);
    }

    public async Task<PagedResult<LabReportResponseDto>> GetPatientReportsAsync(Guid patientUserId, PaginationFilter filter, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.FirstOrDefaultAsync(p => p.UserId == patientUserId, ct)
            ?? throw new NotFoundException("Patient profile not found.");

        var query = _uow.LabReports.Query()
            .Include(l => l.Patient)
            .Include(l => l.Doctor)
            .Where(l => l.PatientId == patient.Id && !l.IsDeleted);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var dtos = items.Select(i => MapToDto(i, i.Patient, i.Doctor)).ToList();

        return PagedResult<LabReportResponseDto>.Create(dtos, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<PagedResult<LabReportResponseDto>> GetDoctorReportsAsync(Guid doctorUserId, PaginationFilter filter, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId, ct)
            ?? throw new NotFoundException("Doctor profile not found.");

        var query = _uow.LabReports.Query()
            .Include(l => l.Patient)
            .Include(l => l.Doctor)
            .Where(l => l.DoctorId == doctor.Id && !l.IsDeleted);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var dtos = items.Select(i => MapToDto(i, i.Patient, i.Doctor)).ToList();

        return PagedResult<LabReportResponseDto>.Create(dtos, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<LabReportResponseDto>> GetConsultationReportsAsync(Guid consultationId, Guid currentUserId, CancellationToken ct = default)
    {
        var reports = await _uow.LabReports.Query()
            .Include(l => l.Patient)
            .Include(l => l.Doctor)
            .Where(l => l.ConsultationId == consultationId && !l.IsDeleted)
            .ToListAsync(ct);

        var accessibleReports = new List<LabReportResponseDto>();
        var isAdmin = _currentUserService.Role == HospitalManagement.DataAccess.Constants.AppConstants.Roles.Admin;

        foreach (var report in reports)
        {
            if (report.IsConfidential && !isAdmin && report.Patient.UserId != currentUserId && report.Doctor.UserId != currentUserId)
            {
                continue; // Skip instead of throwing, so we return what they are allowed to see
            }
            accessibleReports.Add(MapToDto(report, report.Patient, report.Doctor));
        }

        return accessibleReports;
    }

    public async Task DeleteLabReportAsync(Guid id, CancellationToken ct = default)
    {
        var report = await _uow.LabReports.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("LabReport", id);

        report.IsDeleted = true;
        report.DeletedAt = DateTime.UtcNow;

        _uow.LabReports.Update(report);
        await _uow.CompleteAsync(ct);
    }

    public async Task<LabReportStatisticsDto> GetStatisticsAsync(CancellationToken ct = default)
    {
        var reports = await _uow.LabReports.Query().Where(l => !l.IsDeleted).ToListAsync(ct);

        return new LabReportStatisticsDto
        {
            TotalReports = reports.Count,
            PendingReports = reports.Count(r => r.Status == LabReportStatus.Ordered || r.Status == LabReportStatus.SampleCollected || r.Status == LabReportStatus.InProgress),
            CompletedReports = reports.Count(r => r.Status == LabReportStatus.Completed),
            ReviewedReports = reports.Count(r => r.Status == LabReportStatus.Reviewed)
        };
    }

    private void CheckConfidentiality(LabReport report, Guid currentUserId)
    {
        if (!report.IsConfidential) return;

        var isAdmin = _currentUserService.Role == HospitalManagement.DataAccess.Constants.AppConstants.Roles.Admin;
        if (isAdmin) return;

        if (report.Patient.UserId == currentUserId || report.Doctor.UserId == currentUserId)
            return;

        throw new HospitalManagement.DataAccess.Exceptions.UnauthorizedAccessException("This lab report is marked as confidential and you do not have permission to access it.");
    }

    private LabReportResponseDto MapToDto(LabReport report, Patient patient, Doctor doctor)
    {
        return new LabReportResponseDto
        {
            Id = report.Id,
            OrderNumber = report.OrderNumber,
            PatientId = report.PatientId,
            PatientName = $"{patient.FirstName} {patient.LastName}",
            ConsultationId = report.ConsultationId,
            DoctorId = report.DoctorId,
            DoctorName = $"Dr. {doctor.FirstName} {doctor.LastName}",
            TestName = report.TestName,
            Priority = report.Priority,
            OrderNotes = report.OrderNotes,
            ReviewNotes = report.ReviewNotes,
            ReportName = report.ReportName,
            ReportType = report.ReportType,
            Observations = report.Observations,
            Status = report.Status.ToString(),
            IsConfidential = report.IsConfidential,
            OriginalFileName = report.OriginalFileName,
            FileSizeBytes = report.FileSizeBytes,
            CreatedAt = report.CreatedAt
        };
    }
}
