using HospitalManagement.BusinessLogic.DTOs.Dashboard;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IDashboardService
{
    Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default);
    Task<DoctorDashboardDto> GetDoctorDashboardAsync(Guid doctorUserId, CancellationToken ct = default);
    Task<PatientDashboardDto> GetPatientDashboardAsync(Guid patientUserId, CancellationToken ct = default);
}
