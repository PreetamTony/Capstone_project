using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.BusinessLogic.DTOs.Visit;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public class VisitService : IVisitService
{
    private readonly IUnitOfWork _uow;
    private readonly IQueueService _queueService;

    public VisitService(IUnitOfWork uow, IQueueService queueService)
    {
        _uow = uow;
        _queueService = queueService;
    }

    public async Task<VisitDto> StartVisitAsync(StartVisitRequestDto request, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.GetByIdAsync(request.PatientId, ct)
            ?? throw new NotFoundException("Patient", request.PatientId);

        var doctor = await _uow.Doctors.GetByIdAsync(request.DoctorId, ct)
            ?? throw new NotFoundException("Doctor", request.DoctorId);

        var visit = new Visit
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            AppointmentId = request.AppointmentId,
            ChiefComplaint = request.ChiefComplaint,
            Status = VisitStatus.CheckedIn,
            CheckInTime = DateTime.UtcNow
        };

        if (request.AppointmentId.HasValue)
        {
            var appointment = await _uow.Appointments.GetByIdAsync(request.AppointmentId.Value, ct);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.InProgress;
                appointment.CheckedInAt = DateTime.UtcNow;
                _uow.Appointments.Update(appointment);
            }
        }

        await _uow.Visits.AddAsync(visit, ct);
        await _uow.CompleteAsync(ct);

        // Add to Queue
        await _queueService.AddToQueueAsync(request.PatientId, request.DoctorId, visit.Id, ct);

        return await GetVisitByIdAsync(visit.Id, ct);
    }

    public async Task<VisitDto> UpdateVisitAsync(Guid visitId, UpdateVisitRequestDto request, CancellationToken ct = default)
    {
        var visit = await _uow.Visits.GetByIdAsync(visitId, ct)
            ?? throw new NotFoundException("Visit", visitId);

        if (visit.Status == VisitStatus.Discharged || visit.Status == VisitStatus.Cancelled)
            throw new BusinessRuleViolationException("VisitClosed", "Cannot update a discharged or cancelled visit.");

        if (request.Diagnosis != null) visit.Diagnosis = request.Diagnosis;
        if (request.ClinicalNotes != null) visit.ClinicalNotes = request.ClinicalNotes;
        
        visit.Status = VisitStatus.InProgress;

        _uow.Visits.Update(visit);
        await _uow.CompleteAsync(ct);

        return await GetVisitByIdAsync(visit.Id, ct);
    }

    public async Task<VisitDto> DischargeVisitAsync(Guid visitId, CancellationToken ct = default)
    {
        var visit = await _uow.Visits.GetByIdAsync(visitId, ct)
            ?? throw new NotFoundException("Visit", visitId);

        if (visit.Status == VisitStatus.Discharged || visit.Status == VisitStatus.Cancelled)
            throw new BusinessRuleViolationException("VisitClosed", "Visit is already discharged or cancelled.");

        visit.Status = VisitStatus.Discharged;
        visit.DischargeTime = DateTime.UtcNow;

        if (visit.AppointmentId.HasValue)
        {
            var appointment = await _uow.Appointments.GetByIdAsync(visit.AppointmentId.Value, ct);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Completed;
                appointment.CompletedAt = DateTime.UtcNow;
                _uow.Appointments.Update(appointment);
            }
        }

        _uow.Visits.Update(visit);
        await _uow.CompleteAsync(ct);

        return await GetVisitByIdAsync(visit.Id, ct);
    }

    public async Task<VisitDto> CancelVisitAsync(Guid visitId, CancellationToken ct = default)
    {
        var visit = await _uow.Visits.GetByIdAsync(visitId, ct)
            ?? throw new NotFoundException("Visit", visitId);

        if (visit.Status == VisitStatus.Discharged || visit.Status == VisitStatus.Cancelled)
            throw new BusinessRuleViolationException("VisitClosed", "Visit is already discharged or cancelled.");

        visit.Status = VisitStatus.Cancelled;
        visit.DischargeTime = DateTime.UtcNow;

        _uow.Visits.Update(visit);
        await _uow.CompleteAsync(ct);

        return await GetVisitByIdAsync(visit.Id, ct);
    }

    public async Task<VisitDto> GetVisitByIdAsync(Guid visitId, CancellationToken ct = default)
    {
        var visit = await _uow.Visits.GetVisitWithDetailsAsync(visitId, ct)
            ?? throw new NotFoundException("Visit", visitId);

        return MapToDto(visit);
    }

    public async Task<PagedResult<VisitDto>> GetVisitsByPatientAsync(Guid patientId, PaginationFilter filter, CancellationToken ct = default)
    {
        var query = _uow.Visits.Query()
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .Where(v => v.PatientId == patientId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(v => v.CheckInTime)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return PagedResult<VisitDto>.Create(items.Select(MapToDto).ToList(), total, filter.PageNumber, filter.PageSize);
    }

    public async Task<PagedResult<VisitDto>> GetVisitsByDoctorAsync(Guid doctorId, PaginationFilter filter, CancellationToken ct = default)
    {
        var query = _uow.Visits.Query()
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .Where(v => v.DoctorId == doctorId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(v => v.CheckInTime)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return PagedResult<VisitDto>.Create(items.Select(MapToDto).ToList(), total, filter.PageNumber, filter.PageSize);
    }

    private static VisitDto MapToDto(Visit visit)
    {
        return new VisitDto
        {
            Id = visit.Id,
            PatientId = visit.PatientId,
            PatientName = visit.Patient != null ? $"{visit.Patient.FirstName} {visit.Patient.LastName}" : string.Empty,
            DoctorId = visit.DoctorId,
            DoctorName = visit.Doctor != null ? $"Dr. {visit.Doctor.FirstName} {visit.Doctor.LastName}" : string.Empty,
            AppointmentId = visit.AppointmentId,
            Status = visit.Status.ToString(),
            CheckInTime = visit.CheckInTime,
            DischargeTime = visit.DischargeTime,
            ChiefComplaint = visit.ChiefComplaint,
            Diagnosis = visit.Diagnosis,
            ClinicalNotes = visit.ClinicalNotes
        };
    }
}
