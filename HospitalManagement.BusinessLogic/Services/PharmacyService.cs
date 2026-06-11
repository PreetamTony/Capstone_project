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
            .Include(p => p.Patient)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == request.PrescriptionId, ct)
            ?? throw new NotFoundException("Prescription", request.PrescriptionId);

        var record = new DispensationRecord
        {
            PrescriptionId = prescription.Id,
            PatientId = prescription.PatientId,
            DispensedAt = DateTime.UtcNow,
            TotalCost = 0
        };

        foreach (var pItem in prescription.Items)
        {
            var med = await _uow.MedicationInventories.FirstOrDefaultAsync(m => m.Name.ToLower() == pItem.MedicationName.ToLower(), ct);
            if (med == null || med.QuantityInStock < pItem.Quantity)
                throw new BusinessRuleViolationException("OutOfStock", $"Insufficient stock for {pItem.MedicationName}.");

            var dispensedItem = new DispensedItem
            {
                Record = record,
                MedicationId = med.Id,
                Quantity = pItem.Quantity,
                UnitPrice = med.UnitPrice
            };

            record.Items.Add(dispensedItem);
            record.TotalCost += (pItem.Quantity * med.UnitPrice);

            med.QuantityInStock -= pItem.Quantity;
            _uow.MedicationInventories.Update(med);
        }

        await _uow.DispensationRecords.AddAsync(record, ct);
        await _uow.CompleteAsync(ct);

        return new DispensationRecordDto
        {
            Id = record.Id,
            PrescriptionId = record.PrescriptionId,
            PatientId = record.PatientId,
            PatientName = $"{prescription.Patient.FirstName} {prescription.Patient.LastName}",
            DispensedAt = record.DispensedAt,
            TotalCost = record.TotalCost,
            Items = record.Items.Select(i => new DispensedItemDto
            {
                Id = i.Id,
                MedicationName = i.Medication?.Name ?? "Unknown",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }
}
