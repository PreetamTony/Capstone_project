using HospitalManagement.BusinessLogic.DTOs.Billing;
using HospitalManagement.BusinessLogic.DTOs.Common;
using HospitalManagement.DataAccess.Constants;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalManagement.BusinessLogic.Services;

/// <summary>
/// Handles bill generation (auto-triggered on appointment completion) and payment processing.
/// </summary>
public class BillingService : IBillingService
{
    private readonly IUnitOfWork _uow;

    private readonly ILogger<BillingService> _logger;

    public BillingService(IUnitOfWork uow, ILogger<BillingService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<BillingResponseDto> GenerateBillForAppointmentAsync(
        Guid visitId, CancellationToken ct = default)
    {
        // Idempotency: don't generate twice
        var existing = await _uow.Bills.FirstOrDefaultAsync(b => b.VisitId == visitId, ct);
        if (existing != null)
            return await MapToDtoAsync(existing, ct);

        var visit = await _uow.Visits.Query()
            .Include(v => v.Doctor)
            .Include(v => v.Patient)
            .FirstOrDefaultAsync(v => v.Id == visitId, ct)
            ?? throw new NotFoundException("Visit", visitId);

        var patient = visit.Patient;
        var consultationFee = visit.Doctor.ConsultationFee;
        var insuranceCoverage = consultationFee * (patient.InsuranceCoveragePercent / 100m);
        var patientResponsibility = consultationFee - insuranceCoverage;

        var bill = new Billing
        {
            VisitId = visitId,
            PatientId = patient.Id,
            Amount = consultationFee,
            InsuranceCoverage = insuranceCoverage,
            PatientResponsibility = patientResponsibility,
            Status = BillingStatus.Pending
        };

        await _uow.Bills.AddAsync(bill, ct);
        await _uow.CompleteAsync(ct);


        _logger.LogInformation("Bill {Id} generated for Visit {VisitId}, Amount: {Amount}",
            bill.Id, visitId, consultationFee);

        return MapToDto(bill, patient);
    }

    /// <inheritdoc/>
    public async Task<BillingResponseDto> ProcessPaymentAsync(
        Guid billId, Guid patientUserId, PaymentRequestDto request, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.FirstOrDefaultAsync(p => p.UserId == patientUserId, ct)
            ?? throw new NotFoundException("Patient profile not found.");

        var bill = await _uow.Bills.Query()
            .Include(b => b.Patient)
            .FirstOrDefaultAsync(b => b.Id == billId, ct)
            ?? throw new NotFoundException("Bill", billId);

        if (bill.PatientId != patient.Id)
            throw new HospitalManagement.DataAccess.Exceptions.UnauthorizedAccessException("You can only pay your own bills.");

        if (bill.Status == BillingStatus.Paid)
            throw new BusinessRuleViolationException("AlreadyPaid", "This bill has already been paid.");

        if (request.Amount < bill.PatientResponsibility)
            throw new BusinessRuleViolationException("InsufficientAmount",
                $"Payment amount {request.Amount:C} is less than the amount due {bill.PatientResponsibility:C}.");

        bill.Status = BillingStatus.Paid;
        bill.PaymentMethod = request.PaymentMethod;
        bill.PaymentDate = DateTime.UtcNow;
        bill.TransactionId = request.TransactionId ?? Guid.NewGuid().ToString("N")[..12].ToUpper();

        _uow.Bills.Update(bill);
        await _uow.CompleteAsync(ct);


        return MapToDto(bill, bill.Patient);
    }

    /// <inheritdoc/>
    public async Task<PagedResult<BillingResponseDto>> GetPatientOutstandingBillsAsync(
        Guid patientUserId, PaginationFilter filter, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.FirstOrDefaultAsync(p => p.UserId == patientUserId, ct)
            ?? throw new NotFoundException("Patient profile not found.");

        var query = _uow.Bills.Query()
            .Where(b => b.PatientId == patient.Id && b.Status == BillingStatus.Pending)
            .Include(b => b.Patient);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return PagedResult<BillingResponseDto>.Create(
            items.Select(b => MapToDto(b, b.Patient)).ToList(),
            total, filter.PageNumber, filter.PageSize);
    }

    /// <inheritdoc/>
    public async Task<BillingResponseDto> GetByIdAsync(Guid billId, CancellationToken ct = default)
    {
        var bill = await _uow.Bills.Query()
            .Include(b => b.Patient)
            .FirstOrDefaultAsync(b => b.Id == billId, ct)
            ?? throw new NotFoundException("Bill", billId);

        return MapToDto(bill, bill.Patient);
    }

    /// <inheritdoc/>
    public async Task<BillingResponseDto> GetByAppointmentIdAsync(Guid visitId, CancellationToken ct = default)
    {
        var bill = await _uow.Bills.Query()
            .Include(b => b.Patient)
            .FirstOrDefaultAsync(b => b.VisitId == visitId, ct)
            ?? throw new NotFoundException($"No bill found for visit {visitId}.");

        return MapToDto(bill, bill.Patient);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<BillingResponseDto> MapToDtoAsync(Billing bill, CancellationToken ct)
    {
        var patient = await _uow.Patients.GetByIdAsync(bill.PatientId, ct)
            ?? throw new NotFoundException("Patient", bill.PatientId);
        return MapToDto(bill, patient);
    }

    private static BillingResponseDto MapToDto(Billing b, Patient p)
        => new()
        {
            Id = b.Id,
            VisitId = b.VisitId,
            PatientId = p.Id,
            PatientName = $"{p.FirstName} {p.LastName}",
            Amount = b.Amount,
            InsuranceCoverage = b.InsuranceCoverage,
            PatientResponsibility = b.PatientResponsibility,
            Status = b.Status.ToString(),
            PaymentMethod = b.PaymentMethod,
            PaymentDate = b.PaymentDate,
            TransactionId = b.TransactionId,
            Notes = b.Notes,
            CreatedAt = b.CreatedAt
        };
}
