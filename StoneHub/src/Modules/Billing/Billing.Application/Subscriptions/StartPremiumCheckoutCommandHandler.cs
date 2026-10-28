using Billing.Application.Dtos;
using Billing.Application.Payments;
using Billing.Domain;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

// Hands the owner a hosted checkout for auto-renewing Premium. Nothing changes plan-wise here:
// Premium starts when the gateway's charge webhook arrives.
public sealed class StartPremiumCheckoutCommandHandler(IPaymentGateway gateway, ISubscriptionRepository subscriptions)
    : IRequestHandler<StartPremiumCheckoutCommand, Result<CheckoutDto>>
{
    public async Task<Result<CheckoutDto>> Handle(StartPremiumCheckoutCommand request, CancellationToken ct)
    {
        var subscription = await subscriptions.GetByUserIdAsync(request.UserId, ct);
        if (subscription is null)
            return Result.NotFound<CheckoutDto>("No subscription found for this account.");
        if (!gateway.IsAvailable)
            return Result.Unavailable<CheckoutDto>("Online payments aren't available right now.");
        if (subscription.IsPremiumActive)
            return Result.Conflict<CheckoutDto>("This account is already on Premium.");

        var reusable = subscription.ReusableCheckoutUrl(DateTimeOffset.UtcNow);
        if (reusable is not null)
            return Result.Success(new CheckoutDto(reusable));

        var checkout = await gateway.CreateRecurringCheckoutAsync(request.UserId, ct);
        var started = subscription.StartCheckout(checkout.GatewaySubscriptionId, checkout.CheckoutUrl, checkout.ExpiresAt);
        if (started.IsFailure)
            return Result.Failure<CheckoutDto>(started);

        await subscriptions.SaveChangesAsync(ct);
        return Result.Success(new CheckoutDto(checkout.CheckoutUrl));
    }
}
