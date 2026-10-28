using FluentValidation;

namespace Billing.Application.Payments;

public sealed class HandlePaymentWebhookCommandValidator : AbstractValidator<HandlePaymentWebhookCommand>
{
    // Matches the ProcessedPaymentEvents key column.
    public const int MaxEventIdLength = 100;

    public HandlePaymentWebhookCommandValidator()
    {
        RuleFor(x => x.EventId).NotEmpty().MaximumLength(MaxEventIdLength);
        RuleFor(x => x.Payload).NotEmpty();
        RuleFor(x => x.Signature).NotEmpty();
    }
}
