using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Prescription;

namespace HospitalManagement.BusinessLogic.Validators;

public class CreatePrescriptionValidator : AbstractValidator<CreatePrescriptionRequestDto>
{
    public CreatePrescriptionValidator()
    {
        RuleFor(x => x.VisitId)
            .NotEmpty().WithMessage("VisitId is required.");

        RuleFor(x => x.MedicationName)
            .NotEmpty().WithMessage("Medication name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Dosage)
            .NotEmpty().WithMessage("Dosage is required.")
            .MaximumLength(100);

        RuleFor(x => x.Frequency)
            .NotEmpty().WithMessage("Frequency is required.")
            .MaximumLength(100);

        RuleFor(x => x.DurationDays)
            .GreaterThan(0).WithMessage("Duration must be at least 1 day.")
            .LessThanOrEqualTo(365).WithMessage("Duration cannot exceed 365 days.");
    }
}
