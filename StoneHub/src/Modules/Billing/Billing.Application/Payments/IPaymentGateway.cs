using Billing.Domain;

namespace Billing.Application.Payments;

// The recurring-payments provider (Razorpay today). Billing's handlers depend on this, never on
// the provider, so swapping or adding a gateway touches Infrastructure only. Provider faults
// (network, 5xx, rejected credentials) throw: they're infrastructure failures, not business ones.
public interface IPaymentGateway
{
    // False when the gateway's credentials or plans aren't configured (e.g. a dev machine).
    bool IsAvailable { get; }

    // Creates an auto-renewing subscription to a paid plan and returns the hosted page where the
    // owner authorises it. withLaunchOffer applies the first-time-subscriber discount, if one is
    // configured for that plan.
    Task<GatewayCheckout> CreateRecurringCheckoutAsync(
        Guid userId, SubscriptionPlan plan, bool withLaunchOffer, CancellationToken ct = default);

    // The current paid period runs to its end; no further charges.
    Task CancelRenewalAsync(string gatewaySubscriptionId, CancellationToken ct = default);

    // Stops the subscription now (admin cancel), so the owner isn't charged for a plan they lost.
    Task CancelImmediatelyAsync(string gatewaySubscriptionId, CancellationToken ct = default);

    bool IsAuthenticWebhook(string payload, string signature);

    // Null when the payload isn't a well-formed event of the expected shape.
    PaymentEvent? ParseWebhook(string payload);

    // Our plan for one of the gateway's plan ids; null if it isn't one we sell.
    SubscriptionPlan? PlanFor(string gatewayPlanId);
}
