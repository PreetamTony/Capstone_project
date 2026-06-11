using HospitalManagement.BusinessLogic.DTOs.Appointment;
using HospitalManagement.BusinessLogic.Services;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using Moq;
using Xunit;

namespace HospitalManagement.Tests.Services;

public class AppointmentServiceTests : TestBase
{
    private readonly AppointmentService _service;

    public AppointmentServiceTests()
    {
        _service = new AppointmentService(Uow, MockBillingService.Object, MockSlotEngine.Object, MockNotificationService.Object, MockCurrentUserService.Object, CreateLogger<AppointmentService>().Object);
    }

    [Fact]
    public async Task BookAppointmentAsync_ValidRequest_BooksAppointment()
    {
        // Arrange
        var patientUserId = Guid.NewGuid();
        var patient = new Patient { Id = Guid.NewGuid(), UserId = patientUserId };
        Context.Patients.Add(patient);
        var doctor = new Doctor { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        Context.Doctors.Add(doctor);
        await Context.SaveChangesAsync();

        MockCurrentUserService.Setup(u => u.UserId).Returns(patientUserId);

        var request = new BookAppointmentRequestDto
        {
            DoctorId = doctor.Id,
            AppointmentTime = DateTime.UtcNow.AddDays(1),
            Reason = "Checkup"
        };

        // Act
        var result = await _service.BookAppointmentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AppointmentStatus.Scheduled.ToString(), result.Status);
        Assert.Single(Context.Appointments);
        MockNotificationService.Verify(n => n.NotifyAppointmentBookedAsync(patient.Id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
