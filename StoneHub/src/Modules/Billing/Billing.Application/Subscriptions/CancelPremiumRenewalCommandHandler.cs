using Billing.Application.Payments;
using Billing.Domain;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

public sealed class CancelPremiumRenewalCommandHandler(IPaymentGateway gateway, ISubscriptionRepository subscriptions)
    : IRequestHandler<CancelPremiumRenewalCommand, Result>
{
    public async Task<Result> Handle(CancelPremiumRenewalCommand request, CancellationToken ct)
    {
        var subscription = await subscriptions.GetByUserIdAsync(request.UserId, ct);
        if (subscription is null)
            return Result.NotFound("No subscription found for this account.");
        if (!subscription.WillRenew || subscription.GatewaySubscriptionId is not { } gatewayId)
            return Result.Conflict("Premium isn't set to renew.");

        // Gateway first: if it refuses, nothing changes here and the owner can retry.
        await gateway.CancelRenewalAsync(gatewayId, ct);
        subscription.StopRenewal();
        await subscriptions.SaveChangesAsync(ct);

        return Result.Success();
    }
}
