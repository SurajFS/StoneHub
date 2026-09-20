using FluentValidation;

namespace Billing.Application.Subscriptions;

public sealed class ActivatePremiumCommandValidator : AbstractValidator<ActivatePremiumCommand>
{
    public ActivatePremiumCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ExpiryDate).GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("Expiry date must be in the future.");
        RuleFor(x => x.PaymentNotes).MaximumLength(500);
    }
}
