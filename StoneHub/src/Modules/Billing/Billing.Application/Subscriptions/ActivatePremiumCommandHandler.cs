using Billing.Domain;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

public sealed class ActivatePremiumCommandHandler(ISubscriptionRepository subscriptions)
    : IRequestHandler<ActivatePremiumCommand, Result>
{
    public async Task<Result> Handle(ActivatePremiumCommand request, CancellationToken ct)
    {
        var subscription = await subscriptions.GetByUserIdAsync(request.UserId, ct);
        if (subscription is null)
            return Result.NotFound("No subscription found for this account.");

        subscription.ActivatePremium(request.ExpiryDate, request.PaymentNotes);
        await subscriptions.SaveChangesAsync(ct);

        return Result.Success();
    }
}
