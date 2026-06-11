using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Appointment;

namespace HospitalManagement.BusinessLogic.Validators;

public class RescheduleAppointmentValidator : AbstractValidator<RescheduleAppointmentRequestDto>
{
    public RescheduleAppointmentValidator()
    {
        RuleFor(x => x.NewAppointmentTime)
            .GreaterThan(System.DateTime.UtcNow).WithMessage("New appointment time must be in the future.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason for rescheduling is required.")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
    }
}
