using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Prescription;

namespace HospitalManagement.BusinessLogic.Services;

public interface IPrescriptionService
{
    Task<PrescriptionResponseDto> CreatePrescriptionAsync(Guid doctorUserId, CreatePrescriptionRequestDto request, CancellationToken ct = default);
    Task<PrescriptionResponseDto> UpdatePrescriptionAsync(Guid prescriptionId, Guid doctorUserId, UpdatePrescriptionRequestDto request, CancellationToken ct = default);
    Task<PrescriptionResponseDto> VoidPrescriptionAsync(Guid prescriptionId, Guid doctorUserId, VoidPrescriptionRequestDto request, CancellationToken ct = default);
    Task<PrescriptionResponseDto> MarkDispensedAsync(Guid prescriptionId, Guid pharmacistUserId, CancellationToken ct = default);
    Task<PagedResult<PrescriptionResponseDto>> GetPatientPrescriptionsAsync(Guid patientId, PaginationFilter filter, CancellationToken ct = default);
    Task<PrescriptionResponseDto> GetByIdAsync(Guid prescriptionId, CancellationToken ct = default);
}
