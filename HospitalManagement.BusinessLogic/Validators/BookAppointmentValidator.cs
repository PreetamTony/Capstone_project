using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Appointment;
using HospitalManagement.DataAccess.Constants;

namespace HospitalManagement.BusinessLogic.Validators;

public class BookAppointmentValidator : AbstractValidator<BookAppointmentRequestDto>
{
    public BookAppointmentValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("Doctor ID is required.");

        RuleFor(x => x.AppointmentTime)
            .NotEmpty().WithMessage("Appointment time is required.")
            .GreaterThan(DateTime.UtcNow).WithMessage("Appointment cannot be in the past.")
            .Must(BeWithinWorkingHours)
                .WithMessage($"Appointments can only be booked between " +
                             $"{AppConstants.Appointment.WorkdayStartHour}:00 AM and " +
                             $"{AppConstants.Appointment.WorkdayEndHour}:00 PM.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason for appointment is required.")
            .MaximumLength(500);
    }

    private static bool BeWithinWorkingHours(DateTime appointmentTime)
    {
        var hour = appointmentTime.ToUniversalTime().Hour;
        return hour >= AppConstants.Appointment.WorkdayStartHour
            && hour < AppConstants.Appointment.WorkdayEndHour;
    }
}
