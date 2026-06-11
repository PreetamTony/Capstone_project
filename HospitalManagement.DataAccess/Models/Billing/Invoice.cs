using HospitalManagement.DataAccess.Models.Enums.Billing;

namespace HospitalManagement.DataAccess.Models.Billing;

public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid VisitId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal InsuranceCoverage { get; set; }
    public decimal PatientResponsibility { get; set; }
    public decimal TotalAmount { get; set; }
    
    public string? Notes { get; set; }

    public Visit Visit { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<InsuranceClaim> InsuranceClaims { get; set; } = new List<InsuranceClaim>();
}
