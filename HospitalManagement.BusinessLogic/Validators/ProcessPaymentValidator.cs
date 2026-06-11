using FluentValidation;
using HospitalManagement.BusinessLogic.DTOs.Billing;

namespace HospitalManagement.BusinessLogic.Validators;

public class ProcessPaymentValidator : AbstractValidator<ProcessPaymentDto>
{
    public ProcessPaymentValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required.")
            .MaximumLength(50).WithMessage("Payment method cannot exceed 50 characters.");

        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("Transaction ID is required.")
            .MaximumLength(100).WithMessage("Transaction ID cannot exceed 100 characters.");
    }
}
