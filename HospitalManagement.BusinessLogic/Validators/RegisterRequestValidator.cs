using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Auth;

namespace HospitalManagement.BusinessLogic.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{7,14}$").WithMessage("Invalid phone number format.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required.")
            .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past.")
            .GreaterThan(DateTime.UtcNow.AddYears(-120)).WithMessage("Invalid date of birth.");

        RuleFor(x => x.EmergencyContactName)
            .NotEmpty().WithMessage("Emergency contact name is required.")
            .MaximumLength(100);

        RuleFor(x => x.EmergencyContactPhone)
            .NotEmpty().WithMessage("Emergency contact phone is required.")
            .Matches(@"^\+?[1-9]\d{7,14}$").WithMessage("Invalid emergency contact phone format.");

        RuleFor(x => x.InsuranceCoveragePercent)
            .InclusiveBetween(0, 100).WithMessage("Insurance coverage must be between 0 and 100.");
    }
}
