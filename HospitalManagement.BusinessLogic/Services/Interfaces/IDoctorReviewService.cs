using HospitalManagement.BusinessLogic.DTOs.Doctor;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IDoctorReviewService
{
    Task<DoctorReviewResponseDto> CreateReviewAsync(CreateDoctorReviewRequestDto request, CancellationToken ct = default);
    Task<List<DoctorReviewResponseDto>> GetReviewsByDoctorIdAsync(Guid doctorId, CancellationToken ct = default);
}
