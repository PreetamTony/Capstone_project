using HospitalManagement.BusinessLogic.DTOs.Patient;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public class PatientService : IPatientService
{
    private readonly IUnitOfWork _uow;

    public PatientService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PatientResponseDto> GetMyProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.Query()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? throw new NotFoundException("Patient profile not found.");

        return MapToDto(patient);
    }

    public async Task<PatientResponseDto> UpdateMyProfileAsync(Guid userId, UpdatePatientRequestDto request, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.Query()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? throw new NotFoundException("Patient profile not found.");

        if (request.Address != null) patient.Address = request.Address;
        if (request.EmergencyContactName != string.Empty) patient.EmergencyContactName = request.EmergencyContactName;
        if (request.EmergencyContactPhone != string.Empty) patient.EmergencyContactPhone = request.EmergencyContactPhone;
        if (request.InsuranceProvider != null) patient.InsuranceProvider = request.InsuranceProvider;
        if (request.InsurancePolicyNumber != null) patient.InsurancePolicyNumber = request.InsurancePolicyNumber;
        if (request.InsuranceCoveragePercent.HasValue) patient.InsuranceCoveragePercent = request.InsuranceCoveragePercent.Value;
        if (request.BloodGroup.HasValue) patient.BloodGroup = request.BloodGroup;

        _uow.Patients.Update(patient);
        await _uow.CompleteAsync(ct);

        return MapToDto(patient);
    }

    public async Task<PatientResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.Query()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Patient", id);

        return MapToDto(patient);
    }

    private static PatientResponseDto MapToDto(Patient p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        FirstName = p.FirstName,
        LastName = p.LastName,
        FullName = p.FullName,
        Age = p.Age,
        DateOfBirth = p.DateOfBirth,
        Gender = p.Gender.ToString(),
        BloodGroup = p.BloodGroup?.ToString(),
        Address = p.Address,
        Email = p.User?.Email ?? string.Empty,
        PhoneNumber = p.User?.PhoneNumber ?? string.Empty,
        EmergencyContactName = p.EmergencyContactName,
        EmergencyContactPhone = p.EmergencyContactPhone,
        InsuranceProvider = p.InsuranceProvider,
        InsurancePolicyNumber = p.InsurancePolicyNumber,
        InsuranceCoveragePercent = p.InsuranceCoveragePercent,
        NoShowCount = p.NoShowCount,
        IsPriority = p.IsPriority,
        CreatedAt = p.CreatedAt
    };
}
