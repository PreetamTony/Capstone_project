using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Prescription;

namespace HospitalManagement.BusinessLogic.Validators;

public class CreatePrescriptionValidator : AbstractValidator<CreatePrescriptionRequestDto>
{
    public CreatePrescriptionValidator()
    {
        RuleFor(x => x.ConsultationId)
            .NotEmpty().WithMessage("ConsultationId is required.");
            
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}
