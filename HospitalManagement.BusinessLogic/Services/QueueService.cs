using HospitalManagement.BusinessLogic.Hubs;
using HospitalManagement.BusinessLogic.DTOs.Queue;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public class QueueService : IQueueService
{
    private readonly IUnitOfWork _uow;
    private readonly IHubContext<QueueHub> _queueHub;

    public QueueService(IUnitOfWork uow, IHubContext<QueueHub> queueHub)
    {
        _uow = uow;
        _queueHub = queueHub;
    }

    public async Task<QueueEntryDto> AddToQueueAsync(Guid patientId, Guid doctorId, Guid visitId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        
        // Generate Token Number for the day for this doctor
        var lastToken = await _uow.QueueEntries.Query()
            .Where(q => q.DoctorId == doctorId && q.CheckedInAt >= today)
            .MaxAsync(q => (int?)q.TokenNumber, ct) ?? 0;

        var token = lastToken + 1;

        var entry = new QueueEntry
        {
            PatientId = patientId,
            DoctorId = doctorId,
            VisitId = visitId,
            TokenNumber = token,
            Status = QueueStatus.Waiting,
            CheckedInAt = DateTime.UtcNow
        };

        await _uow.QueueEntries.AddAsync(entry, ct);
        await _uow.CompleteAsync(ct);

        return await GetQueueEntryDtoAsync(entry.Id, ct);
    }

    public async Task<CurrentQueueDto> GetCurrentQueueAsync(Guid doctorId, CancellationToken ct = default)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(doctorId, ct)
            ?? throw new NotFoundException("Doctor", doctorId);

        var today = DateTime.UtcNow.Date;
        var entries = await _uow.QueueEntries.Query()
            .Include(q => q.Patient)
            .Where(q => q.DoctorId == doctorId && q.CheckedInAt >= today)
            .OrderBy(q => q.TokenNumber)
            .ToListAsync(ct);

        var current = entries.FirstOrDefault(q => q.Status == QueueStatus.InConsultation);
        var waiting = entries.Where(q => q.Status == QueueStatus.Waiting).ToList();

        return new CurrentQueueDto
        {
            DoctorId = doctor.Id,
            DoctorName = $"Dr. {doctor.FirstName} {doctor.LastName}",
            CurrentPatient = current != null ? MapToDto(current, doctor) : null,
            WaitingPatients = waiting.Select(q => MapToDto(q, doctor)).ToList(),
            TotalWaiting = waiting.Count
        };
    }

    public async Task<QueueEntryDto> CallNextAsync(Guid doctorId, Guid calledByUserId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        // Complete any current patient implicitly if needed, but best practice is explicit.
        var current = await _uow.QueueEntries.Query()
            .FirstOrDefaultAsync(q => q.DoctorId == doctorId && q.Status == QueueStatus.InConsultation && q.CheckedInAt >= today, ct);

        if (current != null)
        {
            current.Status = QueueStatus.Completed;
            current.ConsultationEndedAt = DateTime.UtcNow;
            _uow.QueueEntries.Update(current);
        }

        var next = await _uow.QueueEntries.Query()
            .Where(q => q.DoctorId == doctorId && q.Status == QueueStatus.Waiting && q.CheckedInAt >= today)
            .OrderBy(q => q.TokenNumber)
            .FirstOrDefaultAsync(ct) ?? throw new BusinessRuleViolationException("EmptyQueue", "No patients waiting in queue.");

        next.Status = QueueStatus.InConsultation;
        next.CalledAt = DateTime.UtcNow;
        next.ConsultationStartedAt = DateTime.UtcNow;
        next.CalledBy = calledByUserId;

        _uow.QueueEntries.Update(next);
        await _uow.CompleteAsync(ct);

        await _queueHub.Clients.All.SendAsync("ReceiveQueueUpdate", $"Token {next.TokenNumber} called by Doctor {doctorId}", ct);

        return await GetQueueEntryDtoAsync(next.Id, ct);
    }

    public async Task<QueueEntryDto> SkipTokenAsync(Guid doctorId, int tokenNumber, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var entry = await _uow.QueueEntries.Query()
            .FirstOrDefaultAsync(q => q.DoctorId == doctorId && q.TokenNumber == tokenNumber && q.CheckedInAt >= today, ct)
            ?? throw new NotFoundException($"Token {tokenNumber} not found today.");

        if (entry.Status != QueueStatus.Waiting && entry.Status != QueueStatus.InConsultation)
            throw new BusinessRuleViolationException("InvalidStatus", "Can only skip waiting or in-consultation patients.");

        entry.Status = QueueStatus.Skipped;
        _uow.QueueEntries.Update(entry);
        await _uow.CompleteAsync(ct);

        await _queueHub.Clients.All.SendAsync("ReceiveQueueUpdate", $"Token {tokenNumber} skipped by Doctor {doctorId}", ct);

        return await GetQueueEntryDtoAsync(entry.Id, ct);
    }

    public async Task<QueueEntryDto> RecallPatientAsync(Guid doctorId, int tokenNumber, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var entry = await _uow.QueueEntries.Query()
            .FirstOrDefaultAsync(q => q.DoctorId == doctorId && q.TokenNumber == tokenNumber && q.CheckedInAt >= today, ct)
            ?? throw new NotFoundException($"Token {tokenNumber} not found today.");

        if (entry.Status != QueueStatus.Skipped && entry.Status != QueueStatus.NoShow)
            throw new BusinessRuleViolationException("InvalidStatus", "Can only recall skipped or no-show patients.");

        entry.Status = QueueStatus.Waiting;
        _uow.QueueEntries.Update(entry);
        await _uow.CompleteAsync(ct);

        await _queueHub.Clients.All.SendAsync("ReceiveQueueUpdate", $"Token {tokenNumber} recalled by Doctor {doctorId}", ct);

        return await GetQueueEntryDtoAsync(entry.Id, ct);
    }

    public async Task<QueueEntryDto> MarkNoShowAsync(Guid queueEntryId, CancellationToken ct = default)
    {
        var entry = await _uow.QueueEntries.GetByIdAsync(queueEntryId, ct)
            ?? throw new NotFoundException("QueueEntry", queueEntryId);

        entry.Status = QueueStatus.NoShow;
        _uow.QueueEntries.Update(entry);
        await _uow.CompleteAsync(ct);

        await _queueHub.Clients.All.SendAsync("ReceiveQueueUpdate", $"QueueEntry {queueEntryId} marked as No-Show", ct);

        return await GetQueueEntryDtoAsync(entry.Id, ct);
    }

    public Task<int> GetEstimatedWaitTimeAsync(Guid doctorId, int positionInQueue, CancellationToken ct = default)
    {
        // Simple heuristic: 15 minutes per patient ahead in queue
        return Task.FromResult(positionInQueue * 15);
    }

    private async Task<QueueEntryDto> GetQueueEntryDtoAsync(Guid queueEntryId, CancellationToken ct)
    {
        var entry = await _uow.QueueEntries.Query()
            .Include(q => q.Patient)
            .Include(q => q.Doctor)
            .FirstOrDefaultAsync(q => q.Id == queueEntryId, ct)
            ?? throw new NotFoundException("QueueEntry", queueEntryId);

        return MapToDto(entry, entry.Doctor);
    }

    private static QueueEntryDto MapToDto(QueueEntry q, Doctor d)
    {
        return new QueueEntryDto
        {
            Id = q.Id,
            PatientId = q.PatientId,
            PatientName = q.Patient != null ? $"{q.Patient.FirstName} {q.Patient.LastName}" : string.Empty,
            DoctorId = d.Id,
            DoctorName = $"Dr. {d.FirstName} {d.LastName}",
            VisitId = q.VisitId,
            TokenNumber = q.TokenNumber,
            Status = q.Status.ToString(),
            CheckedInAt = q.CheckedInAt,
            CalledAt = q.CalledAt,
            ConsultationStartedAt = q.ConsultationStartedAt,
            ConsultationEndedAt = q.ConsultationEndedAt
        };
    }
}
