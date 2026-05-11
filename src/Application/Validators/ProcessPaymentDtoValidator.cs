using Application.DTOs.Payments;
using FluentValidation;

namespace Application.Validators;

public class ProcessPaymentDtoValidator : AbstractValidator<ProcessPaymentDto>
{
    public ProcessPaymentDtoValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero.")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Payment amount cannot have more than 2 decimal places.");
    }
}
