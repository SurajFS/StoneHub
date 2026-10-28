using Billing.Application.Payments;
using Billing.Domain;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

public sealed class ActivatePremiumCommandHandler(IPaymentGateway gateway, ISubscriptionRepository subscriptions)
    : IRequestHandler<ActivatePremiumCommand, Result>
{
    public async Task<Result> Handle(ActivatePremiumCommand request, CancellationToken ct)
    {
        var subscription = await subscriptions.GetByUserIdAsync(request.UserId, ct);
        if (subscription is null)
            return Result.NotFound("No subscription found for this account.");

        // An offline payment replaces gateway billing: stop the auto-renewal now, or the owner is
        // charged again for Premium they've already paid for. Gateway first, so a refusal there
        // leaves everything unchanged and the admin can retry.
        if (subscription.WillRenew && subscription.GatewaySubscriptionId is { } gatewayId)
            await gateway.CancelImmediatelyAsync(gatewayId, ct);

        subscription.ActivatePremium(request.ExpiryDate, request.PaymentNotes);
        await subscriptions.SaveChangesAsync(ct);

        return Result.Success();
    }
}
