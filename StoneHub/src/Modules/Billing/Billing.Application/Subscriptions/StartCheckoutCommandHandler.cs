using Billing.Application.Dtos;
using Billing.Application.Payments;
using Billing.Domain;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

// Hands the owner a hosted checkout for an auto-renewing paid plan. Nothing changes plan-wise
// here: the plan starts when the gateway's charge webhook arrives.
public sealed class StartCheckoutCommandHandler(IPaymentGateway gateway, ISubscriptionRepository subscriptions)
    : IRequestHandler<StartCheckoutCommand, Result<CheckoutDto>>
{
    public async Task<Result<CheckoutDto>> Handle(StartCheckoutCommand request, CancellationToken ct)
    {
        if (!gateway.IsAvailable)
            return Result.Unavailable<CheckoutDto>("Online payments aren't available right now.");

        // Owners registered before Billing existed have no row yet; the controller only lets
        // Sellers/Wholesalers here, so it's safe to give them their Free row now.
        var subscription = await subscriptions.GetByUserIdAsync(request.UserId, ct);
        if (subscription is null)
        {
            subscription = Subscription.CreateFree(request.UserId);
            subscriptions.Add(subscription);
        }

        if (!subscription.CanStartCheckout)
            return Result.Conflict<CheckoutDto>("Your current plan renews automatically. Stop auto-renewal first, then subscribe to the other plan.");

        var reusable = subscription.ReusableCheckoutUrl(request.Plan, DateTimeOffset.UtcNow);
        if (reusable is not null)
            return Result.Success(new CheckoutDto(reusable));

        var checkout = await gateway.CreateRecurringCheckoutAsync(
            request.UserId, request.Plan, withLaunchOffer: !subscription.HasEverPaid, ct);
        var started = subscription.StartCheckout(request.Plan, checkout.GatewaySubscriptionId, checkout.CheckoutUrl, checkout.ExpiresAt);
        if (started.IsFailure)
            return Result.Failure<CheckoutDto>(started);

        await subscriptions.SaveChangesAsync(ct);
        return Result.Success(new CheckoutDto(checkout.CheckoutUrl));
    }
}
