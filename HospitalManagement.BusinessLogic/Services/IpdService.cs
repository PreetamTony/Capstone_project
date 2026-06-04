using HospitalManagement.BusinessLogic.DTOs.Ipd;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public class IpdService : IIpdService
{
    private readonly IUnitOfWork _uow;

    public IpdService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<WardDto>> GetAllWardsAsync(CancellationToken ct = default)
    {
        var wards = await _uow.Wards.GetAllAsync(ct);
        return wards.Select(w => new WardDto
        {
            Id = w.Id,
            Name = w.Name,
            Type = w.Type,
            Capacity = w.Capacity
        }).ToList();
    }

    public async Task<List<BedDto>> GetAvailableBedsAsync(Guid wardId, CancellationToken ct = default)
    {
        var beds = await _uow.Beds.Query()
            .Where(b => b.WardId == wardId && !b.IsOccupied)
            .ToListAsync(ct);

        return beds.Select(b => new BedDto
        {
            Id = b.Id,
            WardId = b.WardId,
            BedNumber = b.BedNumber,
            IsOccupied = b.IsOccupied,
            DailyRate = b.DailyRate
        }).ToList();
    }

    public async Task<AdmissionRecordDto> AdmitPatientAsync(AdmitPatientRequestDto request, CancellationToken ct = default)
    {
        var bed = await _uow.Beds.GetByIdAsync(request.BedId, ct)
            ?? throw new NotFoundException("Bed", request.BedId);

        if (bed.IsOccupied)
            throw new BusinessRuleViolationException("BedOccupied", "This bed is already occupied.");

        var patient = await _uow.Patients.GetByIdAsync(request.PatientId, ct) ?? throw new NotFoundException("Patient", request.PatientId);
        var doctor = await _uow.Doctors.GetByIdAsync(request.DoctorId, ct) ?? throw new NotFoundException("Doctor", request.DoctorId);

        bed.IsOccupied = true;
        _uow.Beds.Update(bed);

        var admission = new AdmissionRecord
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            BedId = request.BedId,
            AdmissionDate = DateTime.UtcNow,
            Status = "Admitted"
        };

        await _uow.AdmissionRecords.AddAsync(admission, ct);
        await _uow.CompleteAsync(ct);

        return new AdmissionRecordDto
        {
            Id = admission.Id,
            PatientId = patient.Id,
            PatientName = $"{patient.FirstName} {patient.LastName}",
            DoctorId = doctor.Id,
            DoctorName = $"Dr. {doctor.FirstName} {doctor.LastName}",
            BedId = bed.Id,
            BedNumber = bed.BedNumber,
            AdmissionDate = admission.AdmissionDate,
            Status = admission.Status
        };
    }

    public async Task<AdmissionRecordDto> DischargePatientAsync(Guid admissionId, DischargePatientRequestDto request, CancellationToken ct = default)
    {
        var admission = await _uow.AdmissionRecords.Query()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Bed)
            .FirstOrDefaultAsync(a => a.Id == admissionId, ct)
            ?? throw new NotFoundException("AdmissionRecord", admissionId);

        if (admission.Status == "Discharged")
            throw new BusinessRuleViolationException("AlreadyDischarged", "Patient is already discharged.");

        admission.Status = "Discharged";
        admission.DischargeDate = DateTime.UtcNow;
        admission.DischargeSummary = request.DischargeSummary;

        admission.Bed.IsOccupied = false;
        _uow.Beds.Update(admission.Bed);
        _uow.AdmissionRecords.Update(admission);

        await _uow.CompleteAsync(ct);

        return new AdmissionRecordDto
        {
            Id = admission.Id,
            PatientId = admission.PatientId,
            PatientName = $"{admission.Patient.FirstName} {admission.Patient.LastName}",
            DoctorId = admission.DoctorId,
            DoctorName = $"Dr. {admission.Doctor.FirstName} {admission.Doctor.LastName}",
            BedId = admission.BedId,
            BedNumber = admission.Bed.BedNumber,
            AdmissionDate = admission.AdmissionDate,
            DischargeDate = admission.DischargeDate,
            Status = admission.Status,
            DischargeSummary = admission.DischargeSummary
        };
    }
}
