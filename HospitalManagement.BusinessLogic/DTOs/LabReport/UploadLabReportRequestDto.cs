using Microsoft.AspNetCore.Http;

namespace HospitalManagement.BusinessLogic.DTOs.LabReport;

public class UploadLabReportRequestDto
{
    public Guid PatientId { get; set; }
    public Guid? VisitId { get; set; }
    public Guid DoctorId { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string? Observations { get; set; }
    public bool IsConfidential { get; set; } = false;
    public IFormFile File { get; set; } = null!;
}
