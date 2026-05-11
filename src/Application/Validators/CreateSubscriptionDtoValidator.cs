using Application.DTOs.Subscriptions;
using FluentValidation;

namespace Application.Validators;

public class CreateSubscriptionDtoValidator : AbstractValidator<CreateSubscriptionDto>
{
    public CreateSubscriptionDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.SubscriptionType)
            .IsInEnum().WithMessage("A valid subscription type is required.");

        RuleFor(x => x.ServiceProviderName)
            .NotEmpty().WithMessage("Service provider name is required.")
            .MaximumLength(200).WithMessage("Service provider name cannot exceed 200 characters.");

        RuleFor(x => x.SubscriptionNumber)
            .NotEmpty().WithMessage("Subscription number is required.")
            .MaximumLength(50).WithMessage("Subscription number cannot exceed 50 characters.");
    }
}
