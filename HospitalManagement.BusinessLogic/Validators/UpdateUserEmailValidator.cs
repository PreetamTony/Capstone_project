using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Admin;

namespace HospitalManagement.BusinessLogic.Validators;

public class UpdateUserEmailValidator : AbstractValidator<UpdateUserEmailRequestDto>
{
    public UpdateUserEmailValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}
