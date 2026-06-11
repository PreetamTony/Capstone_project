using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Admin;

namespace HospitalManagement.BusinessLogic.Validators;

public class CreateDoctorProfileValidator : AbstractValidator<CreateDoctorProfileRequestDto>
{
    public CreateDoctorProfileValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department ID is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.Specialization)
            .MaximumLength(100).WithMessage("Specialization cannot exceed 100 characters.");

        RuleFor(x => x.Qualification)
            .MaximumLength(100).WithMessage("Qualification cannot exceed 100 characters.");

        RuleFor(x => x.ExperienceYears)
            .GreaterThanOrEqualTo(0).WithMessage("Experience years must be a non-negative number.");

        RuleFor(x => x.ConsultationFee)
            .GreaterThanOrEqualTo(0).WithMessage("Consultation fee must be a non-negative number.");
    }
}
