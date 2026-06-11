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
    private readonly IQueueHubDispatcher _queueDispatcher;
    private readonly INotificationService _notificationService;

    public QueueService(IUnitOfWork uow, IQueueHubDispatcher queueDispatcher, INotificationService notificationService)
    {
        _uow = uow;
        _queueDispatcher = queueDispatcher;
        _notificationService = notificationService;
    }

    public async Task<QueueEntryDto> AddToQueueAsync(Guid patientId, Guid doctorId, Guid visitId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        // Rule 2: Only One Active Queue Entry Per Visit
        var existing = await _uow.QueueEntries.Query()
            .FirstOrDefaultAsync(q => q.VisitId == visitId && q.Status != QueueStatus.Completed && q.Status != QueueStatus.NoShow, ct);
        
        if (existing != null)
            return await GetQueueEntryDtoAsync(existing.Id, ct);
        
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

        var current = entries.FirstOrDefault(q => q.Status == QueueStatus.InConsultation || q.Status == QueueStatus.Called);
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

    public async Task<QueueEntryDto> CallNextAsync(Guid doctorId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        // Rule 3: Doctor Cannot Call Next While Current Consultation Active
        var current = await _uow.QueueEntries.Query()
            .FirstOrDefaultAsync(q => q.DoctorId == doctorId && (q.Status == QueueStatus.InConsultation || q.Status == QueueStatus.Called) && q.CheckedInAt >= today, ct);

        if (current != null)
            throw new BusinessRuleViolationException("ActiveConsultation", "Cannot call next patient while a consultation is active.");

        var next = await _uow.QueueEntries.Query()
            .Where(q => q.DoctorId == doctorId && q.Status == QueueStatus.Waiting && q.CheckedInAt >= today)
            .OrderBy(q => q.TokenNumber)
            .FirstOrDefaultAsync(ct) ?? throw new BusinessRuleViolationException("EmptyQueue", "No patients waiting in queue.");

        next.Status = QueueStatus.Called;
        next.CallCount += 1;
        next.CalledAt = DateTime.UtcNow;

        _uow.QueueEntries.Update(next);
        await _uow.CompleteAsync(ct);

        await _queueDispatcher.BroadcastQueueUpdateAsync(doctorId, $"Token {next.TokenNumber} called by Doctor {doctorId}", ct);

        // Fetch doctor details for notification
        var doctor = await _uow.Doctors.GetByIdAsync(doctorId, ct);
        var doctorName = doctor != null ? $"Dr. {doctor.FirstName} {doctor.LastName}" : "your Doctor";

        await _notificationService.NotifyQueueCalledAsync(next.PatientId, next.TokenNumber, doctorName, ct);

        return await GetQueueEntryDtoAsync(next.Id, ct);
    }

    public async Task<QueueEntryDto> SkipTokenAsync(Guid doctorId, int tokenNumber, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var entry = await _uow.QueueEntries.Query()
            .FirstOrDefaultAsync(q => q.DoctorId == doctorId && q.TokenNumber == tokenNumber && q.CheckedInAt >= today, ct)
            ?? throw new NotFoundException($"Token {tokenNumber} not found today.");

        if (entry.Status != QueueStatus.Waiting && entry.Status != QueueStatus.Called && entry.Status != QueueStatus.InConsultation)
            throw new BusinessRuleViolationException("InvalidStatus", "Can only skip waiting, called, or in-consultation patients.");

        entry.Status = QueueStatus.Skipped;
        _uow.QueueEntries.Update(entry);
        await _uow.CompleteAsync(ct);

        await _queueDispatcher.BroadcastQueueUpdateAsync(doctorId, $"Token {tokenNumber} skipped by Doctor {doctorId}", ct);

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

        // Re-calling implies putting them back to 'Called' status if there is no one else, but typical hospital flow
        // puts them to Called directly or back to waiting. Let's make them Called directly.
        var current = await _uow.QueueEntries.Query()
            .FirstOrDefaultAsync(q => q.DoctorId == doctorId && (q.Status == QueueStatus.InConsultation || q.Status == QueueStatus.Called) && q.CheckedInAt >= today, ct);

        if (current != null)
            throw new BusinessRuleViolationException("ActiveConsultation", "Cannot recall patient while a consultation is active.");

        entry.Status = QueueStatus.Called;
        entry.CallCount += 1;
        entry.CalledAt = DateTime.UtcNow;

        _uow.QueueEntries.Update(entry);
        await _uow.CompleteAsync(ct);

        await _queueDispatcher.BroadcastQueueUpdateAsync(doctorId, $"Token {tokenNumber} recalled by Doctor {doctorId}", ct);

        return await GetQueueEntryDtoAsync(entry.Id, ct);
    }

    public async Task<QueueEntryDto> MarkNoShowAsync(Guid queueEntryId, CancellationToken ct = default)
    {
        var entry = await _uow.QueueEntries.GetByIdAsync(queueEntryId, ct)
            ?? throw new NotFoundException("QueueEntry", queueEntryId);

        // Rule 4: NoShow Only After Multiple Calls
        if (entry.CallCount < 3)
            throw new BusinessRuleViolationException("InsufficientCalls", $"Patient must be called at least 3 times before marking as No-Show. Current Call Count: {entry.CallCount}. Please skip and recall later.");

        entry.Status = QueueStatus.NoShow;
        _uow.QueueEntries.Update(entry);
        await _uow.CompleteAsync(ct);

        await _queueDispatcher.BroadcastQueueUpdateAsync(entry.DoctorId, $"QueueEntry {queueEntryId} marked as No-Show", ct);

        return await GetQueueEntryDtoAsync(entry.Id, ct);
    }

    public async Task<QueueEntryDto> RejoinQueueAsync(Guid queueEntryId, CancellationToken ct = default)
    {
        var entry = await _uow.QueueEntries.GetByIdAsync(queueEntryId, ct)
            ?? throw new NotFoundException("QueueEntry", queueEntryId);

        if (entry.Status != QueueStatus.Skipped && entry.Status != QueueStatus.NoShow)
            throw new BusinessRuleViolationException("InvalidStatus", "Only skipped or no-show patients can rejoin the queue.");

        entry.Status = QueueStatus.Waiting;
        _uow.QueueEntries.Update(entry);
        await _uow.CompleteAsync(ct);

        await _queueDispatcher.BroadcastQueueUpdateAsync(entry.DoctorId, $"Token {entry.TokenNumber} rejoined queue", ct);

        return await GetQueueEntryDtoAsync(entry.Id, ct);
    }

    public async Task<QueueEntryDto> CompleteQueueEntryAsync(Guid queueEntryId, CancellationToken ct = default)
    {
        var entry = await _uow.QueueEntries.GetByIdAsync(queueEntryId, ct)
            ?? throw new NotFoundException("QueueEntry", queueEntryId);

        entry.Status = QueueStatus.Completed;
        entry.ConsultationEndedAt = DateTime.UtcNow;

        _uow.QueueEntries.Update(entry);
        await _uow.CompleteAsync(ct);

        await _queueDispatcher.BroadcastQueueUpdateAsync(entry.DoctorId, $"QueueEntry {queueEntryId} completed", ct);

        return await GetQueueEntryDtoAsync(entry.Id, ct);
    }

    public Task<int> GetEstimatedWaitTimeAsync(Guid doctorId, int positionInQueue, CancellationToken ct = default)
    {
        return Task.FromResult(positionInQueue * 15);
    }

    public async Task<QueuePositionDto> GetMyPositionAsync(Guid patientUserId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        
        var patient = await _uow.Patients.Query()
            .FirstOrDefaultAsync(p => p.UserId == patientUserId, ct)
            ?? throw new NotFoundException("Patient account not found.");

        var activeEntry = await _uow.QueueEntries.Query()
            .Include(q => q.Doctor)
            .Where(q => q.PatientId == patient.Id && q.CheckedInAt >= today && (q.Status == QueueStatus.Waiting || q.Status == QueueStatus.Called || q.Status == QueueStatus.InConsultation))
            .OrderByDescending(q => q.CheckedInAt)
            .FirstOrDefaultAsync(ct) ?? throw new NotFoundException("No active queue entry found for today.");

        if (activeEntry.Status == QueueStatus.Called || activeEntry.Status == QueueStatus.InConsultation)
        {
            return new QueuePositionDto
            {
                TokenNumber = activeEntry.TokenNumber,
                Position = 0,
                EstimatedWaitMinutes = 0,
                DoctorName = $"Dr. {activeEntry.Doctor.FirstName} {activeEntry.Doctor.LastName}"
            };
        }

        var waitingCountAhead = await _uow.QueueEntries.Query()
            .CountAsync(q => q.DoctorId == activeEntry.DoctorId && q.CheckedInAt >= today && q.Status == QueueStatus.Waiting && q.TokenNumber < activeEntry.TokenNumber, ct);

        return new QueuePositionDto
        {
            TokenNumber = activeEntry.TokenNumber,
            Position = waitingCountAhead + 1,
            EstimatedWaitMinutes = (waitingCountAhead + 1) * 15,
            DoctorName = $"Dr. {activeEntry.Doctor.FirstName} {activeEntry.Doctor.LastName}"
        };
    }

    public async Task<QueueDisplayDto> GetDisplayQueueAsync(Guid doctorId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        var currentToken = await _uow.QueueEntries.Query()
            .Where(q => q.DoctorId == doctorId && q.CheckedInAt >= today && (q.Status == QueueStatus.Called || q.Status == QueueStatus.InConsultation))
            .OrderByDescending(q => q.CalledAt ?? q.CheckedInAt)
            .Select(q => q.TokenNumber)
            .FirstOrDefaultAsync(ct);

        var nextToken = await _uow.QueueEntries.Query()
            .Where(q => q.DoctorId == doctorId && q.CheckedInAt >= today && q.Status == QueueStatus.Waiting)
            .OrderBy(q => q.TokenNumber)
            .Select(q => q.TokenNumber)
            .FirstOrDefaultAsync(ct);

        return new QueueDisplayDto
        {
            CurrentToken = currentToken,
            NextToken = nextToken
        };
    }

    public async Task<QueueStatisticsDto> GetQueueStatisticsAsync(DateTime date, CancellationToken ct = default)
    {
        var targetDate = date.Date;
        var nextDate = targetDate.AddDays(1);

        var query = _uow.QueueEntries.Query().Where(q => q.CheckedInAt >= targetDate && q.CheckedInAt < nextDate);

        var entries = await query.ToListAsync(ct);

        var patientsWaiting = entries.Count(q => q.Status == QueueStatus.Waiting);
        var noShows = entries.Count(q => q.Status == QueueStatus.NoShow);
        var patientsServed = entries.Count(q => q.Status == QueueStatus.Completed);

        var servedEntriesWithTime = entries.Where(q => q.Status == QueueStatus.Completed && q.CalledAt.HasValue).ToList();
        var avgWaitMinutes = servedEntriesWithTime.Any() 
            ? (int)servedEntriesWithTime.Average(q => (q.CalledAt!.Value - q.CheckedInAt).TotalMinutes) 
            : 0;

        return new QueueStatisticsDto
        {
            AverageWaitTime = avgWaitMinutes,
            PatientsWaiting = patientsWaiting,
            PatientsServedToday = patientsServed,
            NoShows = noShows
        };
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
