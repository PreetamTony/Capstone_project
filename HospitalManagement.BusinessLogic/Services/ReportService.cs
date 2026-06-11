using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow;

    public ReportService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<object> GetRevenueReportAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var bills = await _uow.Invoices.Query()
            .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
            .ToListAsync(ct);

        var totalRevenue = bills.Sum(b => b.TotalAmount);
        var totalPaid = bills.Where(b => b.Status == HospitalManagement.DataAccess.Models.Enums.Billing.InvoiceStatus.Paid).Sum(b => b.TotalAmount);
        
        return new
        {
            TotalRevenue = totalRevenue,
            TotalPaid = totalPaid,
            TotalPending = totalRevenue - totalPaid,
            Count = bills.Count
        };
    }

    public async Task<object> GetAppointmentReportAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var appointments = await _uow.Appointments.Query()
            .Where(a => a.AppointmentTime >= startDate && a.AppointmentTime <= endDate)
            .ToListAsync(ct);

        return new
        {
            TotalAppointments = appointments.Count,
            Completed = appointments.Count(a => a.Status == HospitalManagement.DataAccess.Models.Enums.AppointmentStatus.Completed),
            Cancelled = appointments.Count(a => a.Status == HospitalManagement.DataAccess.Models.Enums.AppointmentStatus.Cancelled),
            NoShow = appointments.Count(a => a.Status == HospitalManagement.DataAccess.Models.Enums.AppointmentStatus.NoShow)
        };
    }
}
