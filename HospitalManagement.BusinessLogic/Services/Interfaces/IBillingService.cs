using HospitalManagement.BusinessLogic.DTOs.Billing;
using HospitalManagement.BusinessLogic.DTOs.Common;

namespace HospitalManagement.BusinessLogic.Services;

public interface IBillingService
{
    Task<BillingResponseDto> GenerateBillForAppointmentAsync(Guid visitId, CancellationToken ct = default);
    Task<BillingResponseDto> ProcessPaymentAsync(Guid billId, Guid patientUserId, PaymentRequestDto request, CancellationToken ct = default);
    Task<PagedResult<BillingResponseDto>> GetPatientOutstandingBillsAsync(Guid patientUserId, PaginationFilter filter, CancellationToken ct = default);
    Task<BillingResponseDto> GetByIdAsync(Guid billId, CancellationToken ct = default);
    Task<BillingResponseDto> GetByAppointmentIdAsync(Guid visitId, CancellationToken ct = default);
}
