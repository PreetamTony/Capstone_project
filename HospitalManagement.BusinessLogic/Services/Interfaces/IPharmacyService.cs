using HospitalManagement.BusinessLogic.DTOs.Pharmacy;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IPharmacyService
{
    Task<List<MedicationInventoryDto>> GetInventoryAsync(CancellationToken ct = default);
    Task<DispensationRecordDto> DispensePrescriptionAsync(DispensePrescriptionRequestDto request, CancellationToken ct = default);
}
