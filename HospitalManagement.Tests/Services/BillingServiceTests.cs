using HospitalManagement.BusinessLogic.DTOs.Billing;
using HospitalManagement.BusinessLogic.Services;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Billing;
using HospitalManagement.DataAccess.Models.Enums.Billing;
using Moq;
using Xunit;

namespace HospitalManagement.Tests.Services;

public class BillingServiceTests : TestBase
{
    private readonly BillingService _service;

    public BillingServiceTests()
    {
        _service = new BillingService(Uow, CreateLogger<BillingService>().Object, MockCurrentUserService.Object, MockNotificationService.Object);
    }

    [Fact]
    public async Task GenerateInvoiceForVisitAsync_WhenVisitExists_GeneratesInvoice()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = new Patient { Id = patientId };
        var doctor = new Doctor { Id = Guid.NewGuid() };
        Context.Patients.Add(patient);
        Context.Doctors.Add(doctor);

        var visit = new Visit
        {
            Id = visitId,
            PatientId = patientId,
            DoctorId = doctor.Id,
            Consultation = new Consultation { Id = Guid.NewGuid() }
        };

        Context.Visits.Add(visit);
        await Context.SaveChangesAsync();

        // Act
        var result = await _service.GenerateInvoiceForVisitAsync(visitId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(InvoiceStatus.Generated.ToString(), result.Status);
        Assert.Single(Context.Invoices);
        MockNotificationService.Verify(n => n.NotifyInvoiceGeneratedAsync(patientId, It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithSufficientAmount_MarksAsPaid()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = new Patient { Id = patientId };
        var doctor = new Doctor { Id = Guid.NewGuid() };
        Context.Patients.Add(patient);
        Context.Doctors.Add(doctor);

        var visit = new Visit { Id = Guid.NewGuid(), PatientId = patientId, DoctorId = doctor.Id };
        Context.Visits.Add(visit);
        await Context.SaveChangesAsync();
        Context.Visits.Add(visit);

        var invoice = new Invoice
        {
            Id = invoiceId,
            Status = InvoiceStatus.Pending,
            TotalAmount = 1000,
            PatientResponsibility = 1000,
            VisitId = visit.Id
        };

        Context.Invoices.Add(invoice);
        
        await Context.SaveChangesAsync();

        var request = new ProcessPaymentDto
        {
            Amount = 1000,
            PaymentMethod = "Cash",
            TransactionId = "TXN-123"
        };

        // Act
        var result = await _service.ProcessPaymentAsync(invoiceId, request);

        // Assert
        Assert.Equal(InvoiceStatus.Paid.ToString(), result.Status);
        
        var updatedInvoice = await Context.Invoices.FindAsync(invoiceId);
        Assert.Equal(InvoiceStatus.Paid, updatedInvoice.Status);
        Assert.Equal(0, updatedInvoice.PatientResponsibility);
        
        MockNotificationService.Verify(n => n.NotifyPaymentReceivedAsync(patientId, invoiceId, 1000, It.IsAny<CancellationToken>()), Times.Once);
    }
}
