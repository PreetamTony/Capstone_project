using HospitalManagement.BusinessLogic.DTOs.Visit;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public class ConsultationService : IConsultationService
{
    private readonly IUnitOfWork _uow;

    public ConsultationService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ConsultationResponseDto> CreateConsultationAsync(CreateConsultationRequestDto request, CancellationToken ct = default)
    {
        var visit = await _uow.Visits.GetByIdAsync(request.VisitId, ct) 
            ?? throw new NotFoundException("Visit", request.VisitId);

        var consultation = new Consultation
        {
            VisitId = request.VisitId,
            Symptoms = request.Symptoms,
            Diagnosis = request.Diagnosis,
            Notes = request.Notes,
            Recommendations = request.Recommendations,
            Status = Enum.TryParse<HospitalManagement.DataAccess.Models.Enums.ConsultationStatus>(request.Status, true, out var parsedStatus) 
                     ? parsedStatus 
                     : HospitalManagement.DataAccess.Models.Enums.ConsultationStatus.Draft
        };

        await _uow.Consultations.AddAsync(consultation, ct);
        await _uow.CompleteAsync(ct);

        return MapToDto(consultation);
    }

    public async Task<ConsultationResponseDto> GetConsultationByIdAsync(Guid id, CancellationToken ct = default)
    {
        var consultation = await _uow.Consultations.GetByIdAsync(id, ct) 
            ?? throw new NotFoundException("Consultation", id);

        return MapToDto(consultation);
    }

    public async Task<List<ConsultationResponseDto>> GetConsultationsByVisitIdAsync(Guid visitId, CancellationToken ct = default)
    {
        var consultations = await _uow.Consultations.Query()
            .Where(c => c.VisitId == visitId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        return consultations.Select(MapToDto).ToList();
    }

    public async Task<ConsultationResponseDto> UpdateConsultationAsync(Guid id, CreateConsultationRequestDto request, CancellationToken ct = default)
    {
        var consultation = await _uow.Consultations.GetByIdAsync(id, ct) 
            ?? throw new NotFoundException("Consultation", id);

        consultation.Symptoms = request.Symptoms;
        consultation.Diagnosis = request.Diagnosis;
        consultation.Notes = request.Notes;
        consultation.Recommendations = request.Recommendations;
        consultation.Status = Enum.TryParse<HospitalManagement.DataAccess.Models.Enums.ConsultationStatus>(request.Status, true, out var parsedStatus) 
                     ? parsedStatus 
                     : HospitalManagement.DataAccess.Models.Enums.ConsultationStatus.Draft;

        _uow.Consultations.Update(consultation);
        await _uow.CompleteAsync(ct);

        return MapToDto(consultation);
    }

    private static ConsultationResponseDto MapToDto(Consultation c) => new()
    {
        Id = c.Id,
        VisitId = c.VisitId,
        Symptoms = c.Symptoms,
        Diagnosis = c.Diagnosis,
        Notes = c.Notes,
        Recommendations = c.Recommendations,
        Status = c.Status.ToString(),
        CreatedAt = c.CreatedAt
    };
}
