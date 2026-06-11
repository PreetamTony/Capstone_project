using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Billing;

namespace HospitalManagement.BusinessLogic.Validators;

public class ProcessRefundValidator : AbstractValidator<ProcessRefundDto>
{
    public ProcessRefundValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Refund amount must be greater than 0.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Refund reason is required.")
            .MaximumLength(500).WithMessage("Refund reason cannot exceed 500 characters.");
    }
}
