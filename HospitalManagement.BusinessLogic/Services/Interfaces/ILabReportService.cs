using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.LabReport;
using Microsoft.AspNetCore.Http;

namespace HospitalManagement.BusinessLogic.Services;

public interface ILabReportService
{
    Task<LabReportResponseDto> UploadReportAsync(Guid uploaderUserId, UploadLabReportRequestDto request, CancellationToken ct = default);
    Task<(byte[] Content, string ContentType, string FileName)> DownloadReportAsync(Guid reportId, Guid requestingUserId, CancellationToken ct = default);
    Task<PagedResult<LabReportResponseDto>> GetPatientReportsAsync(Guid patientId, PaginationFilter filter, CancellationToken ct = default);
    Task<LabReportResponseDto> UpdateStatusAsync(Guid reportId, string status, CancellationToken ct = default);
    Task<LabReportResponseDto> GetByIdAsync(Guid reportId, CancellationToken ct = default);
}
