using HospitalManagement.BusinessLogic.DTOs.Dashboard;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Models.Enums.Billing;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _uow;

    public DashboardService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        
        var totalPatients = await _uow.Patients.Query().CountAsync(ct);
        var totalDoctors = await _uow.Doctors.Query().CountAsync(ct);
        
        var appointmentsToday = await _uow.Appointments.Query()
            .Where(a => a.AppointmentTime.Date == today)
            .CountAsync(ct);

        var pendingReports = await _uow.LabReports.Query()
            .Where(r => r.Status == LabReportStatus.Ordered || r.Status == LabReportStatus.SampleCollected || r.Status == LabReportStatus.InProgress)
            .CountAsync(ct);

        // Calculate today's revenue from Paid bills
        var billsToday = await _uow.Invoices.Query()
            .Where(b => b.Status == InvoiceStatus.Paid && b.CreatedAt.Date == today)
            .SumAsync(b => b.TotalAmount, ct);

        return new AdminDashboardDto
        {
            TotalPatients = totalPatients,
            TotalDoctors = totalDoctors,
            AppointmentsToday = appointmentsToday,
            PendingLabReports = pendingReports,
            TotalRevenueToday = billsToday
        };
    }

    public async Task<DoctorDashboardDto> GetDoctorDashboardAsync(Guid doctorUserId, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId, ct)
            ?? throw new NotFoundException("Doctor not found.");

        var today = DateTime.UtcNow.Date;
        var appointments = await _uow.Appointments.Query()
            .Where(a => a.DoctorId == doctor.Id && a.AppointmentTime.Date == today)
            .ToListAsync(ct);

        return new DoctorDashboardDto
        {
            PatientsToday = appointments.Count(a => a.Status != AppointmentStatus.Cancelled),
            PendingConsultations = appointments.Count(a => a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.CheckedIn || a.Status == AppointmentStatus.Confirmed),
            CompletedConsultations = appointments.Count(a => a.Status == AppointmentStatus.Completed),
            CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled || a.Status == AppointmentStatus.NoShow)
        };
    }

    public async Task<PatientDashboardDto> GetPatientDashboardAsync(Guid patientUserId, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.FirstOrDefaultAsync(p => p.UserId == patientUserId, ct)
            ?? throw new NotFoundException("Patient not found.");

        var today = DateTime.UtcNow;

        var upcomingAppointments = await _uow.Appointments.Query()
            .Where(a => a.PatientId == patient.Id && a.AppointmentTime > today && a.Status != AppointmentStatus.Cancelled)
            .CountAsync(ct);

        var unpaidBills = await _uow.Invoices.Query()
            .Where(b => b.PatientId == patient.Id && b.Status == InvoiceStatus.Pending)
            .CountAsync(ct);

        var newReports = await _uow.LabReports.Query()
            .Where(r => r.PatientId == patient.Id && r.Status == LabReportStatus.Completed && r.CreatedAt >= today.AddDays(-7))
            .CountAsync(ct);

        return new PatientDashboardDto
        {
            UpcomingAppointments = upcomingAppointments,
            UnpaidBills = unpaidBills,
            NewLabReports = newReports
        };
    }
}
