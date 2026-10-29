using Billing.Domain;
using FluentValidation;

namespace Billing.Application.Subscriptions;

public sealed class StartCheckoutCommandValidator : AbstractValidator<StartCheckoutCommand>
{
    public StartCheckoutCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Plan).IsInEnum().NotEqual(SubscriptionPlan.Free)
            .WithMessage("Choose Basic or Premium.");
    }
}
