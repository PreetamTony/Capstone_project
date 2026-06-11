using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Prescription;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IPrescriptionService
{
    Task<PrescriptionResponseDto> CreatePrescriptionAsync(Guid doctorUserId, CreatePrescriptionRequestDto request, CancellationToken ct = default);
    Task<PrescriptionItemDto> AddMedicationItemAsync(Guid prescriptionId, Guid doctorUserId, AddMedicationItemRequestDto request, CancellationToken ct = default);
    Task<PrescriptionItemDto> UpdateMedicationItemAsync(Guid itemId, Guid doctorUserId, UpdateMedicationItemRequestDto request, CancellationToken ct = default);
    Task DeleteMedicationItemAsync(Guid itemId, Guid doctorUserId, CancellationToken ct = default);
    
    Task<PrescriptionResponseDto> GetByIdAsync(Guid prescriptionId, CancellationToken ct = default);
    Task<List<PrescriptionSummaryDto>> GetByConsultationAsync(Guid consultationId, CancellationToken ct = default);
    Task<PagedResult<PrescriptionSummaryDto>> GetPatientPrescriptionsAsync(Guid patientUserId, PaginationFilter filter, CancellationToken ct = default);
    Task<PagedResult<PrescriptionSummaryDto>> GetDoctorPrescriptionsAsync(Guid doctorUserId, PaginationFilter filter, CancellationToken ct = default);
    
    Task<PrescriptionResponseDto> FinalizePrescriptionAsync(Guid prescriptionId, Guid doctorUserId, CancellationToken ct = default);
    Task<PrescriptionResponseDto> DispensePrescriptionAsync(Guid prescriptionId, Guid pharmacistUserId, CancellationToken ct = default);
    Task<PrescriptionResponseDto> VoidPrescriptionAsync(Guid prescriptionId, Guid doctorUserId, VoidPrescriptionRequestDto request, CancellationToken ct = default);
}
