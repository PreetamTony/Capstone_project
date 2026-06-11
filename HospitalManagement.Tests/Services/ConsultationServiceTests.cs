using HospitalManagement.BusinessLogic.DTOs.Visit;
using HospitalManagement.BusinessLogic.Services;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using Moq;
using Xunit;

namespace HospitalManagement.Tests.Services;

public class ConsultationServiceTests : TestBase
{
    private readonly ConsultationService _service;

    public ConsultationServiceTests()
    {
        _service = new ConsultationService(Uow, MockCurrentUserService.Object, MockBillingService.Object, MockNotificationService.Object);
    }

    [Fact]
    public async Task CreateConsultationAsync_ValidRequest_CreatesConsultation()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var doctor = new Doctor { Id = Guid.NewGuid(), UserId = doctorUserId };
        var visit = new Visit
        {
            Id = visitId,
            DoctorId = doctor.Id,
            Doctor = doctor,
            Status = VisitStatus.CheckedIn
        };

        Context.Doctors.Add(doctor);
        Context.Visits.Add(visit);
        await Context.SaveChangesAsync();

        MockCurrentUserService.Setup(u => u.UserId).Returns(doctorUserId);

        var request = new CreateConsultationRequestDto
        {
            VisitId = visitId,
            Symptoms = new List<string> { "Headache" }
        };

        // Act
        var result = await _service.CreateConsultationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ConsultationStatus.InProgress.ToString(), result.Status);
        Assert.Single(Context.Consultations);
    }

    [Fact]
    public async Task CompleteConsultationAsync_ValidConsultation_Completes()
    {
        // Arrange
        var consultationId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var doctor = new Doctor { Id = Guid.NewGuid(), UserId = doctorUserId };
        
        var consultation = new Consultation
        {
            Id = consultationId, Diagnosis = "Healthy", TreatmentPlan = "Rest",
            Status = ConsultationStatus.InProgress,
            DoctorId = doctor.Id,
            Doctor = doctor,
            Visit = new Visit { PatientId = Guid.NewGuid(), DoctorId = doctor.Id, Doctor = doctor }
        };

        Context.Doctors.Add(doctor);
        Context.Consultations.Add(consultation);
        await Context.SaveChangesAsync();

        MockCurrentUserService.Setup(u => u.UserId).Returns(doctorUserId);

        // Act
        var result = await _service.CompleteConsultationAsync(consultationId);

        // Assert
        Assert.Equal(ConsultationStatus.Completed.ToString(), result.Status);
        
        var updatedConsultation = await Context.Consultations.FindAsync(consultationId);
        Assert.Equal(ConsultationStatus.Completed, updatedConsultation.Status);
    }
}
