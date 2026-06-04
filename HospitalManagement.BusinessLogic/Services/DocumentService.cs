using HospitalManagement.BusinessLogic.DTOs.Patient;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _uow;

    public DocumentService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<DocumentResponseDto> CreateDocumentAsync(CreateDocumentRequestDto request, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.GetByIdAsync(request.PatientId, ct)
            ?? throw new NotFoundException("Patient", request.PatientId);

        var doc = new Document
        {
            PatientId = request.PatientId,
            DocumentType = request.DocumentType,
            FileName = request.FileName,
            FileUrl = request.FileUrl,
            FileSizeBytes = request.FileSizeBytes
        };

        await _uow.Documents.AddAsync(doc, ct);
        await _uow.CompleteAsync(ct);

        return MapToDto(doc);
    }

    public async Task<DocumentResponseDto> GetDocumentByIdAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await _uow.Documents.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Document", id);

        return MapToDto(doc);
    }

    public async Task<List<DocumentResponseDto>> GetDocumentsByPatientIdAsync(Guid patientId, CancellationToken ct = default)
    {
        var docs = await _uow.Documents.Query()
            .Where(d => d.PatientId == patientId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

        return docs.Select(MapToDto).ToList();
    }

    private static DocumentResponseDto MapToDto(Document d) => new()
    {
        Id = d.Id,
        PatientId = d.PatientId,
        DocumentType = d.DocumentType,
        FileName = d.FileName,
        FileUrl = d.FileUrl,
        FileSizeBytes = d.FileSizeBytes,
        CreatedAt = d.CreatedAt
    };
}
