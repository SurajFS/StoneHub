using Billing.Domain;
using FluentValidation;

namespace Billing.Application.Subscriptions;

public sealed class ActivatePlanCommandValidator : AbstractValidator<ActivatePlanCommand>
{
    public ActivatePlanCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Plan).IsInEnum().NotEqual(SubscriptionPlan.Free)
            .WithMessage("Choose Basic or Premium.");
        RuleFor(x => x.ExpiryDate).GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("Expiry date must be in the future.");
        RuleFor(x => x.PaymentNotes).MaximumLength(500);
    }
}
