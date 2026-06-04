using HospitalManagement.BusinessLogic.DTOs.Ipd;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IIpdService
{
    Task<List<WardDto>> GetAllWardsAsync(CancellationToken ct = default);
    Task<List<BedDto>> GetAvailableBedsAsync(Guid wardId, CancellationToken ct = default);
    Task<AdmissionRecordDto> AdmitPatientAsync(AdmitPatientRequestDto request, CancellationToken ct = default);
    Task<AdmissionRecordDto> DischargePatientAsync(Guid admissionId, DischargePatientRequestDto request, CancellationToken ct = default);
}
