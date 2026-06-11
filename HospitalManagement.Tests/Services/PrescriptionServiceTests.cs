using HospitalManagement.BusinessLogic.DTOs.Prescription;
using HospitalManagement.BusinessLogic.Services;
using HospitalManagement.DataAccess.Models;
using HospitalManagement.DataAccess.Models.Enums;
using Moq;
using Xunit;

namespace HospitalManagement.Tests.Services;

public class PrescriptionServiceTests : TestBase
{
    private readonly PrescriptionService _service;

    public PrescriptionServiceTests()
    {
        _service = new PrescriptionService(Uow, CreateLogger<PrescriptionService>().Object, MockNotificationService.Object);
    }

    [Fact]
    public async Task CreatePrescriptionAsync_WhenConsultationInProgress_CreatesPrescription()
    {
        // Arrange
        var consultationId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var doctor = new Doctor { Id = Guid.NewGuid(), UserId = doctorUserId };
        var patient = new Patient { Id = Guid.NewGuid() };
        var consultation = new Consultation 
        { 
            Id = consultationId, 
            Status = ConsultationStatus.InProgress,
            DoctorId = doctor.Id,
            Doctor = doctor,
            Visit = new Visit { PatientId = patient.Id, Patient = patient }
        };

        Context.Doctors.Add(doctor);
        Context.Patients.Add(patient);
        Context.Consultations.Add(consultation);
        await Context.SaveChangesAsync();

        var request = new CreatePrescriptionRequestDto
        {
            ConsultationId = consultationId,
            Notes = "Take after meals"
        };

        // Act
        var result = await _service.CreatePrescriptionAsync(doctorUserId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrescriptionStatus.Draft.ToString(), result.Status);
        Assert.Single(Context.Prescriptions);
    }

    [Fact]
    public async Task FinalizePrescriptionAsync_WithItems_FinalizesAndNotifies()
    {
        // Arrange
        var prescriptionId = Guid.NewGuid();
        var doctorUserId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = new Patient { Id = patientId };
        var doctor = new Doctor { Id = Guid.NewGuid(), UserId = doctorUserId };
        Context.Patients.Add(patient);
        Context.Doctors.Add(doctor);

        var prescription = new Prescription
        {
            Id = prescriptionId,
            Status = PrescriptionStatus.Draft,
            DoctorId = doctor.Id,
            PatientId = patientId,
            Items = new List<PrescriptionItem> 
            { 
                new PrescriptionItem { MedicationName = "Paracetamol" } 
            }
        };

        Context.Prescriptions.Add(prescription);
        await Context.SaveChangesAsync();

        // Act
        var result = await _service.FinalizePrescriptionAsync(prescriptionId, doctorUserId);

        // Assert
        Assert.Equal(PrescriptionStatus.Active.ToString(), result.Status);
        Assert.NotNull(result.FinalizedAt);
        
        var updatedPrescription = await Context.Prescriptions.FindAsync(prescriptionId);
        Assert.Equal(PrescriptionStatus.Active, updatedPrescription.Status);
        
        MockNotificationService.Verify(n => n.NotifyPrescriptionCreatedAsync(patientId, prescriptionId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
