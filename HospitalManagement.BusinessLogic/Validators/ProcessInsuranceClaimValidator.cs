using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Billing;

namespace HospitalManagement.BusinessLogic.Validators;

public class ProcessInsuranceClaimValidator : AbstractValidator<ProcessInsuranceClaimDto>
{
    public ProcessInsuranceClaimValidator()
    {
        RuleFor(x => x.ClaimAmount)
            .GreaterThan(0).WithMessage("Claim amount must be greater than zero.");

        RuleFor(x => x.InsuranceProvider)
            .NotEmpty().WithMessage("Insurance provider is required.")
            .MaximumLength(100).WithMessage("Provider name cannot exceed 100 characters.");

        RuleFor(x => x.PolicyNumber)
            .NotEmpty().WithMessage("Policy number is required.")
            .MaximumLength(100).WithMessage("Policy number cannot exceed 100 characters.");
    }
}
