namespace HospitalManagement.BusinessLogic.DTOs.Pharmacy;

public class MedicationInventoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int QuantityInStock { get; set; }
    public int ReorderLevel { get; set; }
}

public class DispensePrescriptionRequestDto
{
    public Guid PrescriptionId { get; set; }
}

public class DispensationRecordDto
{
    public Guid Id { get; set; }
    public Guid PrescriptionId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime DispensedAt { get; set; }
    public decimal TotalCost { get; set; }
    public List<DispensedItemDto> Items { get; set; } = new();
}

public class DispensedItemDto
{
    public Guid Id { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
