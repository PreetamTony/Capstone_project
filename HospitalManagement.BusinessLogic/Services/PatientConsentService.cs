using HospitalManagement.BusinessLogic.DTOs.Patient;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public class PatientConsentService : IPatientConsentService
{
    private readonly IUnitOfWork _uow;

    public PatientConsentService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<PatientConsentResponseDto>> GetConsentsByPatientIdAsync(Guid patientId, CancellationToken ct = default)
    {
        var consents = await _uow.PatientConsents.Query()
            .Where(c => c.PatientId == patientId)
            .ToListAsync(ct);

        return consents.Select(MapToDto).ToList();
    }

    public async Task<PatientConsentResponseDto> UpdateConsentAsync(Guid patientId, UpdatePatientConsentRequestDto request, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.GetByIdAsync(patientId, ct)
            ?? throw new NotFoundException("Patient", patientId);

        var existing = await _uow.PatientConsents.Query()
            .FirstOrDefaultAsync(c => c.PatientId == patientId && c.ConsentType == request.ConsentType, ct);

        if (existing == null)
        {
            existing = new PatientConsent
            {
                PatientId = patientId,
                ConsentType = request.ConsentType,
                IsGranted = request.IsGranted,
                GrantedAt = request.IsGranted ? DateTime.UtcNow : DateTime.MinValue,
                RevokedAt = !request.IsGranted ? DateTime.UtcNow : null
            };
            await _uow.PatientConsents.AddAsync(existing, ct);
        }
        else
        {
            // Only update if changed
            if (existing.IsGranted != request.IsGranted)
            {
                existing.IsGranted = request.IsGranted;
                if (request.IsGranted)
                {
                    existing.GrantedAt = DateTime.UtcNow;
                    existing.RevokedAt = null;
                }
                else
                {
                    existing.RevokedAt = DateTime.UtcNow;
                }
                _uow.PatientConsents.Update(existing);
            }
        }

        await _uow.CompleteAsync(ct);
        return MapToDto(existing);
    }

    private static PatientConsentResponseDto MapToDto(PatientConsent c) => new()
    {
        Id = c.Id,
        PatientId = c.PatientId,
        ConsentType = c.ConsentType,
        IsGranted = c.IsGranted,
        GrantedAt = c.GrantedAt,
        RevokedAt = c.RevokedAt
    };
}
