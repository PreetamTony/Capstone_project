using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Appointment;

namespace HospitalManagement.BusinessLogic.Validators;

public class CancelAppointmentValidator : AbstractValidator<CancelAppointmentRequestDto>
{
    public CancelAppointmentValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Cancellation reason is required.")
            .MinimumLength(5).WithMessage("Reason must be at least 5 characters long.")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
    }
}
