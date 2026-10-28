using Billing.Application.Payments;
using Billing.Domain;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

public sealed class CancelSubscriptionCommandHandler(IPaymentGateway gateway, ISubscriptionRepository subscriptions)
    : IRequestHandler<CancelSubscriptionCommand, Result>
{
    public async Task<Result> Handle(CancelSubscriptionCommand request, CancellationToken ct)
    {
        var subscription = await subscriptions.GetByUserIdAsync(request.UserId, ct);
        if (subscription is null)
            return Result.NotFound("No subscription found for this account.");

        // Admin cancel is immediate, so the gateway must stop charging now too; otherwise the
        // owner keeps paying for Premium they no longer have.
        if (subscription.WillRenew && subscription.GatewaySubscriptionId is { } gatewayId)
            await gateway.CancelImmediatelyAsync(gatewayId, ct);

        subscription.Cancel();
        await subscriptions.SaveChangesAsync(ct);

        return Result.Success();
    }
}
