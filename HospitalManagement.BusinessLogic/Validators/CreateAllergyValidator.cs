using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Emr;

namespace HospitalManagement.BusinessLogic.Validators;

public class CreateAllergyValidator : AbstractValidator<CreateAllergyRequestDto>
{
    public CreateAllergyValidator()
    {
        RuleFor(x => x.Substance)
            .NotEmpty().WithMessage("Allergen/Substance cannot be empty.")
            .MaximumLength(100).WithMessage("Substance name cannot exceed 100 characters.");

        RuleFor(x => x.Severity)
            .NotEmpty().WithMessage("Severity must be specified (e.g., Mild, Moderate, Severe).")
            .MaximumLength(50).WithMessage("Severity cannot exceed 50 characters.");

        RuleFor(x => x.Reaction)
            .NotEmpty().WithMessage("Reaction must be specified.")
            .MaximumLength(200).WithMessage("Reaction cannot exceed 200 characters.");
            
        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
    }
}
