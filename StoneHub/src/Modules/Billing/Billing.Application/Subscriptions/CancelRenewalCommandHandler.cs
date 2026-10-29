using Billing.Application.Payments;
using Billing.Domain;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

public sealed class CancelRenewalCommandHandler(IPaymentGateway gateway, ISubscriptionRepository subscriptions)
    : IRequestHandler<CancelRenewalCommand, Result>
{
    public async Task<Result> Handle(CancelRenewalCommand request, CancellationToken ct)
    {
        var subscription = await subscriptions.GetByUserIdAsync(request.UserId, ct);
        if (subscription is null)
            return Result.NotFound("No subscription found for this account.");
        if (!subscription.WillRenew || subscription.GatewaySubscriptionId is not { } gatewayId)
            return Result.Conflict("Your plan isn't set to renew.");

        // Gateway first: if it refuses, nothing changes here and the owner can retry.
        await gateway.CancelRenewalAsync(gatewayId, ct);
        subscription.StopRenewal();
        await subscriptions.SaveChangesAsync(ct);

        return Result.Success();
    }
}
