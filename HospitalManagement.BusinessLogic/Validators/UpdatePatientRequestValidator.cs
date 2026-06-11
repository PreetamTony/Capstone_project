using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Patient;

namespace HospitalManagement.BusinessLogic.Validators;

public class UpdatePatientRequestValidator : AbstractValidator<UpdatePatientRequestDto>
{
    public UpdatePatientRequestValidator()
    {
        RuleFor(x => x.EmergencyContactName)
            .NotEmpty().WithMessage("Emergency contact name is required.")
            .MaximumLength(100).WithMessage("Emergency contact name cannot exceed 100 characters.");

        RuleFor(x => x.EmergencyContactPhone)
            .NotEmpty().WithMessage("Emergency contact phone is required.")
            .Matches(@"^\+?[1-9]\d{7,14}$").WithMessage("Invalid emergency contact phone format.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.");

        RuleFor(x => x.InsuranceProvider)
            .MaximumLength(100).WithMessage("Insurance provider name cannot exceed 100 characters.");

        RuleFor(x => x.InsurancePolicyNumber)
            .MaximumLength(100).WithMessage("Insurance policy number cannot exceed 100 characters.");

        RuleFor(x => x.InsuranceCoveragePercent)
            .InclusiveBetween(0, 100).WithMessage("Insurance coverage must be between 0 and 100.")
            .When(x => x.InsuranceCoveragePercent.HasValue);
    }
}
