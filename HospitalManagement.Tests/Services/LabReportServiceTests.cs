using HospitalManagement.BusinessLogic.DTOs.LabReport;
using HospitalManagement.BusinessLogic.Services;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using Moq;
using Xunit;

namespace HospitalManagement.Tests.Services;

public class LabReportServiceTests : TestBase
{
    private readonly LabReportService _service;

    public LabReportServiceTests()
    {
        MockCurrentUserService.Setup(u => u.Role).Returns("Doctor");
        _service = new LabReportService(Uow, CreateLogger<LabReportService>().Object, MockNotificationService.Object, MockCurrentUserService.Object);
    }

    [Fact]
    public async Task CreateLabOrderAsync_WhenConsultationInProgress_CreatesOrder()
    {
        // Arrange
        var consultationId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = new Patient { Id = patientId };
        var doctor = new Doctor { Id = doctorId, UserId = doctorUserId };
        Context.Patients.Add(patient);
        Context.Doctors.Add(doctor);
        var consultation = new Consultation 
        { 
            Id = consultationId, 
            Status = ConsultationStatus.InProgress,
            DoctorId = doctorId,
            Doctor = doctor,
            Visit = new Visit { PatientId = patientId, Patient = patient }
        };

        Context.Doctors.Add(doctor);
        Context.Patients.Add(patient);
        Context.Consultations.Add(consultation);
        await Context.SaveChangesAsync();

        MockCurrentUserService.Setup(u => u.UserId).Returns(doctorUserId);

        var request = new CreateLabOrderRequestDto
        {
            ConsultationId = consultationId,
            TestName = "CBC",
            Priority = "Urgent",
            Notes = "Check for infection"
        };

        // Act
        var result = await _service.CreateLabOrderAsync(doctorUserId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(LabReportStatus.Ordered.ToString(), result.Status);
        Assert.StartsWith("LAB-", result.OrderNumber);
        Assert.Single(Context.LabReports);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ToCompleted_NotifiesDoctorAndPatient()
    {
        // Arrange
        var labReportId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var patient = new Patient { Id = patientId };
        var doctor = new Doctor { Id = doctorId };
        Context.Patients.Add(patient);
        Context.Doctors.Add(doctor);

        var labReport = new LabReport
        {
            Id = labReportId,
            Status = LabReportStatus.InProgress,
            PatientId = patientId,
            DoctorId = doctorId
        };

        Context.LabReports.Add(labReport);
        await Context.SaveChangesAsync();

        var request = new UpdateLabReportStatusDto
        {
            Status = LabReportStatus.Completed.ToString()
        };

        // Act
        var result = await _service.UpdateOrderStatusAsync(labReportId, request);

        // Assert
        Assert.Equal(LabReportStatus.Completed.ToString(), result.Status);
        
        var updatedReport = await Context.LabReports.FindAsync(labReportId);
        Assert.Equal(LabReportStatus.Completed, updatedReport.Status);
        
        MockNotificationService.Verify(n => n.NotifyReportUploadedAsync(patientId, labReportId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
