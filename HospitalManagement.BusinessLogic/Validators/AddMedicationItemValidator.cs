using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Prescription;

namespace HospitalManagement.BusinessLogic.Validators;

public class AddMedicationItemValidator : AbstractValidator<AddMedicationItemRequestDto>
{
    public AddMedicationItemValidator()
    {
        RuleFor(x => x.MedicationName)
            .NotEmpty().WithMessage("Medication name is required.")
            .MaximumLength(200).WithMessage("Medication name cannot exceed 200 characters.");

        RuleFor(x => x.Dosage)
            .NotEmpty().WithMessage("Dosage is required.")
            .MaximumLength(100).WithMessage("Dosage cannot exceed 100 characters.");

        RuleFor(x => x.Frequency)
            .NotEmpty().WithMessage("Frequency is required.")
            .MaximumLength(100).WithMessage("Frequency cannot exceed 100 characters.");

        RuleFor(x => x.DurationDays)
            .GreaterThan(0).WithMessage("Duration in days must be greater than 0.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
    }
}
