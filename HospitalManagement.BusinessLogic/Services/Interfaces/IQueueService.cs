using HospitalManagement.BusinessLogic.DTOs.Queue;

namespace HospitalManagement.BusinessLogic.Services.Interfaces;

public interface IQueueService
{
    Task<QueueEntryDto> AddToQueueAsync(Guid patientId, Guid doctorId, Guid visitId, CancellationToken ct = default);
    Task<CurrentQueueDto> GetCurrentQueueAsync(Guid doctorId, CancellationToken ct = default);
    Task<QueueEntryDto> CallNextAsync(Guid doctorId, Guid calledByUserId, CancellationToken ct = default);
    Task<QueueEntryDto> SkipTokenAsync(Guid doctorId, int tokenNumber, CancellationToken ct = default);
    Task<QueueEntryDto> RecallPatientAsync(Guid doctorId, int tokenNumber, CancellationToken ct = default);
    Task<QueueEntryDto> MarkNoShowAsync(Guid queueEntryId, CancellationToken ct = default);
    Task<int> GetEstimatedWaitTimeAsync(Guid doctorId, int positionInQueue, CancellationToken ct = default);
}
