using Billing.Application.Payments;
using Billing.Domain;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

public sealed class ActivatePlanCommandHandler(IPaymentGateway gateway, ISubscriptionRepository subscriptions)
    : IRequestHandler<ActivatePlanCommand, Result>
{
    public async Task<Result> Handle(ActivatePlanCommand request, CancellationToken ct)
    {
        var subscription = await subscriptions.GetByUserIdAsync(request.UserId, ct);
        if (subscription is null)
            return Result.NotFound("No subscription found for this account.");

        // An offline payment replaces gateway billing: stop the auto-renewal now, or the owner is
        // charged again for a plan they've already paid for. Gateway first, so a refusal there
        // leaves everything unchanged and the admin can retry.
        if (subscription.WillRenew && subscription.GatewaySubscriptionId is { } gatewayId)
            await gateway.CancelImmediatelyAsync(gatewayId, ct);

        var activated = subscription.ActivatePlan(request.Plan, request.ExpiryDate, request.PaymentNotes);
        if (activated.IsFailure)
            return activated;

        await subscriptions.SaveChangesAsync(ct);
        return Result.Success();
    }
}
