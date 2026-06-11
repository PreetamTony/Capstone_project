using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.LabReport;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface ILabReportService
{
    // Orders
    Task<LabReportResponseDto> CreateLabOrderAsync(Guid doctorUserId, CreateLabOrderRequestDto request, CancellationToken ct = default);
    Task<LabReportResponseDto> UpdateOrderStatusAsync(Guid orderId, UpdateLabReportStatusDto request, CancellationToken ct = default);
    
    // Reports
    Task<LabReportResponseDto> UploadLabReportAsync(Guid reportId, Guid uploaderUserId, UploadLabReportRequestDto request, CancellationToken ct = default);
    Task<LabReportResponseDto> ReviewLabReportAsync(Guid reportId, Guid reviewerUserId, ReviewLabReportDto request, CancellationToken ct = default);
    
    // Queries
    Task<LabReportResponseDto> GetByIdAsync(Guid id, Guid currentUserId, CancellationToken ct = default);
    Task<(byte[] fileBytes, string contentType, string fileName)> DownloadReportAsync(Guid id, Guid currentUserId, CancellationToken ct = default);
    Task<PagedResult<LabReportResponseDto>> GetPatientReportsAsync(Guid patientUserId, PaginationFilter filter, CancellationToken ct = default);
    Task<PagedResult<LabReportResponseDto>> GetDoctorReportsAsync(Guid doctorUserId, PaginationFilter filter, CancellationToken ct = default);
    Task<List<LabReportResponseDto>> GetConsultationReportsAsync(Guid consultationId, Guid currentUserId, CancellationToken ct = default);
    
    // Admin / Management
    Task DeleteLabReportAsync(Guid id, CancellationToken ct = default);
    Task<LabReportStatisticsDto> GetStatisticsAsync(CancellationToken ct = default);
}
