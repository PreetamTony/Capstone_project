using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.LabReport;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Constants;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.BusinessLogic.Services;

/// <summary>
/// Handles lab report file uploads to wwwroot/uploads/labreports and metadata persistence.
/// </summary>
public class LabReportService : ILabReportService
{
    private readonly IUnitOfWork _uow;

    private readonly INotificationService _notificationService;
    private readonly ILogger<LabReportService> _logger;

    // Use absolute path for storage in development (capstone scope)
    private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "lab-reports");

    public LabReportService(IUnitOfWork uow, 
        INotificationService notificationService, ILogger<LabReportService> logger)
    {
        _uow = uow;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<LabReportResponseDto> UploadReportAsync(
        Guid uploaderUserId, UploadLabReportRequestDto request, CancellationToken ct = default)
    {
        // Validate file
        var file = request.File;
        if (file == null || file.Length == 0)
            throw new BusinessRuleViolationException("EmptyFile", "No file was provided.");

        if (file.Length > AppConstants.File.MaxFileSizeBytes)
            throw new BusinessRuleViolationException("FileTooLarge",
                $"File size exceeds the maximum allowed size of {AppConstants.File.MaxFileSizeBytes / 1024 / 1024} MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AppConstants.File.AllowedExtensions.Contains(ext))
            throw new BusinessRuleViolationException("InvalidFileType",
                $"File type '{ext}' is not allowed. Allowed types: {string.Join(", ", AppConstants.File.AllowedExtensions)}");

        // Validate entities exist
        var patient = await _uow.Patients.GetByIdAsync(request.PatientId, ct)
            ?? throw new NotFoundException("Patient", request.PatientId);

        var doctor = await _uow.Doctors.GetByIdAsync(request.DoctorId, ct)
            ?? throw new NotFoundException("Doctor", request.DoctorId);

        // Save file
        var uploadDir = AppConstants.File.LabReportUploadPath;
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        var report = new LabReport
        {
            PatientId = request.PatientId,
            VisitId = request.VisitId,
            DoctorId = request.DoctorId,
            ReportName = request.ReportName,
            ReportType = request.ReportType,
            FilePath = filePath,
            OriginalFileName = file.FileName,
            FileSizeBytes = file.Length,
            Observations = request.Observations,
            Status = LabReportStatus.Pending,
            IsConfidential = request.IsConfidential,
            UploadedBy = uploaderUserId
        };

        await _uow.LabReports.AddAsync(report, ct);
        await _uow.CompleteAsync(ct);


        _logger.LogInformation("Lab report {Id} uploaded by {UserId} for patient {PatientId}",
            report.Id, uploaderUserId, request.PatientId);

        // Notify patient
        await _notificationService.NotifyReportUploadedAsync(request.PatientId, report.Id, ct);

        return MapToDto(report, patient, doctor);
    }

    /// <inheritdoc/>
    public async Task<(byte[] Content, string ContentType, string FileName)> DownloadReportAsync(
        Guid reportId, Guid requestingUserId, CancellationToken ct = default)
    {
        var report = await _uow.LabReports.Query()
            .Include(r => r.Patient)
            .FirstOrDefaultAsync(r => r.Id == reportId, ct)
            ?? throw new NotFoundException("Lab report", reportId);

        // Confidential reports: only requesting patient or uploader can download
        if (report.IsConfidential && report.Patient.UserId != requestingUserId && report.UploadedBy != requestingUserId)
            throw new HospitalManagement.DataAccess.Exceptions.UnauthorizedAccessException("This report is confidential.");

        if (!File.Exists(report.FilePath))
            throw new NotFoundException($"Report file not found on disk for report {reportId}.");

        var content = await File.ReadAllBytesAsync(report.FilePath, ct);
        var ext = Path.GetExtension(report.FilePath).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

        return (content, contentType, report.OriginalFileName ?? Path.GetFileName(report.FilePath));
    }

    /// <inheritdoc/>
    public async Task<PagedResult<LabReportResponseDto>> GetPatientReportsAsync(
        Guid patientId, PaginationFilter filter, CancellationToken ct = default)
    {
        var query = _uow.LabReports.Query()
            .Where(r => r.PatientId == patientId)
            .Include(r => r.Patient)
            .Include(r => r.Doctor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(r => r.ReportName.Contains(filter.SearchTerm) || r.ReportType.Contains(filter.SearchTerm));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return PagedResult<LabReportResponseDto>.Create(
            items.Select(r => MapToDto(r, r.Patient, r.Doctor)).ToList(),
            total, filter.PageNumber, filter.PageSize);
    }

    /// <inheritdoc/>
    public async Task<LabReportResponseDto> UpdateStatusAsync(Guid reportId, string status, CancellationToken ct = default)
    {
        var report = await _uow.LabReports.Query()
            .Include(r => r.Patient)
            .Include(r => r.Doctor)
            .FirstOrDefaultAsync(r => r.Id == reportId, ct)
            ?? throw new NotFoundException("Lab report", reportId);

        if (!Enum.TryParse<LabReportStatus>(status, true, out var newStatus))
            throw new BusinessRuleViolationException("InvalidStatus", $"Invalid lab report status: {status}");

        report.Status = newStatus;
        _uow.LabReports.Update(report);
        await _uow.CompleteAsync(ct);

        // Notify patient
        await _notificationService.NotifyReportUploadedAsync(report.Patient.Id, report.Id, ct);

        return MapToDto(report, report.Patient, report.Doctor);
    }

    /// <inheritdoc/>
    public async Task<LabReportResponseDto> GetByIdAsync(Guid reportId, CancellationToken ct = default)
    {
        var report = await _uow.LabReports.Query()
            .Include(r => r.Patient)
            .Include(r => r.Doctor)
            .FirstOrDefaultAsync(r => r.Id == reportId, ct)
            ?? throw new NotFoundException("Lab report", reportId);

        return MapToDto(report, report.Patient, report.Doctor);
    }

    private static LabReportResponseDto MapToDto(LabReport r, Patient p, Doctor d)
        => new()
        {
            Id = r.Id,
            PatientId = p.Id,
            PatientName = $"{p.FirstName} {p.LastName}",
            VisitId = r.VisitId,
            DoctorId = d.Id,
            DoctorName = $"Dr. {d.FirstName} {d.LastName}",
            ReportName = r.ReportName,
            ReportType = r.ReportType,
            Observations = r.Observations,
            Status = r.Status.ToString(),
            IsConfidential = r.IsConfidential,
            OriginalFileName = r.OriginalFileName,
            FileSizeBytes = r.FileSizeBytes,
            CreatedAt = r.CreatedAt
        };
}
