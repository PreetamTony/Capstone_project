using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Department;

namespace HospitalManagement.BusinessLogic.Validators;

public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentRequestDto>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required.")
            .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}
