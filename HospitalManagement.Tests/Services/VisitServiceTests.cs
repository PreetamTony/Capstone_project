using HospitalManagement.BusinessLogic.DTOs.Visit;
using HospitalManagement.BusinessLogic.Services;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using Moq;
using Xunit;

namespace HospitalManagement.Tests.Services;

public class VisitServiceTests : TestBase
{
    private readonly VisitService _service;

    public VisitServiceTests()
    {
        _service = new VisitService(Uow, MockQueueService.Object, MockBillingService.Object, MockCurrentUserService.Object, CreateLogger<VisitService>().Object);
    }

    [Fact]
    public async Task StartVisitAsync_ValidAppointment_CreatesVisit()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var patient = new Patient { Id = Guid.NewGuid(), UserId = patientUserId };
        var doctor = new Doctor { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        
        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            Status = AppointmentStatus.Scheduled,
            AppointmentTime = DateTime.UtcNow.AddDays(1)
        };

        Context.Patients.Add(patient);
        Context.Doctors.Add(doctor);
        Context.Appointments.Add(appointment);
        await Context.SaveChangesAsync();

        var request = new StartVisitRequestDto
        {
            Notes = "Arrived early"
        };

        // Act
        var result = await _service.StartVisitAsync(appointmentId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("CheckedIn", result.Status);
        Assert.Single(Context.Visits);
        
        // Ensure appointment is checked in
        var updatedAppointment = await Context.Appointments.FindAsync(appointmentId);
        Assert.Equal(AppointmentStatus.CheckedIn, updatedAppointment.Status);
    }

    [Fact]
    public async Task DischargeVisitAsync_ValidVisit_DischargesAndGeneratesInvoice()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var patient = new Patient { Id = Guid.NewGuid() };
        var doctor = new Doctor { Id = Guid.NewGuid() };
        Context.Patients.Add(patient);
        Context.Doctors.Add(doctor);
        var visit = new Visit
        {
            Id = visitId,
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            Status = VisitStatus.InConsultation
        };

        Context.Visits.Add(visit);
        await Context.SaveChangesAsync();

        // Act
        var result = await _service.DischargeVisitAsync(visitId);

        // Assert
        Assert.Equal(VisitStatus.Completed.ToString(), result.Status);
        
        var updatedVisit = await Context.Visits.FindAsync(visitId);
        Assert.Equal(VisitStatus.Completed, updatedVisit.Status);
        
        MockBillingService.Verify(b => b.GenerateInvoiceForVisitAsync(visitId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
