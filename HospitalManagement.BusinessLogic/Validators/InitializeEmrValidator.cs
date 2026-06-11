using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Emr;

namespace HospitalManagement.BusinessLogic.Validators;

public class InitializeEmrValidator : AbstractValidator<InitializeEmrRequestDto>
{
    public InitializeEmrValidator()
    {
        RuleFor(x => x.FamilyHistory)
            .MaximumLength(1000).WithMessage("Family history cannot exceed 1000 characters.");

        RuleFor(x => x.SocialHistory)
            .MaximumLength(1000).WithMessage("Social history cannot exceed 1000 characters.");
    }
}
