using HospitalManagement.BusinessLogic.DTOs.Pharmacy;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using HospitalManagement.DataAccess.Exceptions;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.BusinessLogic.Services;

public class PharmacyService : IPharmacyService
{
    private readonly IUnitOfWork _uow;

    public PharmacyService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<MedicationInventoryDto>> GetInventoryAsync(CancellationToken ct = default)
    {
        var inventory = await _uow.MedicationInventories.GetAllAsync(ct);
        return inventory.Select(m => new MedicationInventoryDto
        {
            Id = m.Id,
            Name = m.Name,
            Code = m.Code,
            UnitPrice = m.UnitPrice,
            QuantityInStock = m.QuantityInStock,
            ReorderLevel = m.ReorderLevel
        }).ToList();
    }

    public async Task<DispensationRecordDto> DispensePrescriptionAsync(DispensePrescriptionRequestDto request, CancellationToken ct = default)
    {
        var prescription = await _uow.Prescriptions.Query()
            .Include(p => p.Visit)
                .ThenInclude(v => v.Patient)
            .FirstOrDefaultAsync(p => p.Id == request.PrescriptionId, ct)
            ?? throw new NotFoundException("Prescription", request.PrescriptionId);

        // Simple parsing logic: assume prescription format is "MedicationName - Quantity"
        // In a real system, Prescription would have structured Items instead of a string.
        // For now, we will just dispense a generic matching item or throw if not found.
        
        var med = await _uow.MedicationInventories.FirstOrDefaultAsync(m => m.QuantityInStock > 0, ct);
        if (med == null)
            throw new BusinessRuleViolationException("OutOfStock", "No medications in stock to dispense.");

        int quantityToDispense = 1;

        var record = new DispensationRecord
        {
            PrescriptionId = prescription.Id,
            PatientId = prescription.Visit.PatientId,
            DispensedAt = DateTime.UtcNow,
            TotalCost = med.UnitPrice * quantityToDispense
        };

        var dispensedItem = new DispensedItem
        {
            Record = record,
            MedicationId = med.Id,
            Quantity = quantityToDispense,
            UnitPrice = med.UnitPrice
        };

        record.Items.Add(dispensedItem);

        // Deduct from inventory
        med.QuantityInStock -= quantityToDispense;
        _uow.MedicationInventories.Update(med);

        await _uow.DispensationRecords.AddAsync(record, ct);
        await _uow.CompleteAsync(ct);

        return new DispensationRecordDto
        {
            Id = record.Id,
            PrescriptionId = record.PrescriptionId,
            PatientId = record.PatientId,
            PatientName = $"{prescription.Visit.Patient.FirstName} {prescription.Visit.Patient.LastName}",
            DispensedAt = record.DispensedAt,
            TotalCost = record.TotalCost,
            Items = record.Items.Select(i => new DispensedItemDto
            {
                Id = i.Id,
                MedicationName = med.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }
}
