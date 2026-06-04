namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IReportService
{
    Task<object> GetRevenueReportAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<object> GetAppointmentReportAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
}
