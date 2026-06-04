using HospitalManagement.BusinessLogic.DTOs.Patient;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IDocumentService
{
    Task<DocumentResponseDto> CreateDocumentAsync(CreateDocumentRequestDto request, CancellationToken ct = default);
    Task<DocumentResponseDto> GetDocumentByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<DocumentResponseDto>> GetDocumentsByPatientIdAsync(Guid patientId, CancellationToken ct = default);
}
