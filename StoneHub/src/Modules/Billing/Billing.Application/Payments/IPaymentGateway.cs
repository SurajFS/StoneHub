namespace Billing.Application.Payments;

// The recurring-payments provider (Razorpay today). Billing's handlers depend on this, never on
// the provider, so swapping or adding a gateway touches Infrastructure only. Provider faults
// (network, 5xx, rejected credentials) throw: they're infrastructure failures, not business ones.
public interface IPaymentGateway
{
    // False when the gateway's credentials or plan aren't configured (e.g. a dev machine).
    bool IsAvailable { get; }

    // Creates an auto-renewing subscription for the user and returns the hosted page where they
    // authorise it.
    Task<GatewayCheckout> CreateRecurringCheckoutAsync(Guid userId, CancellationToken ct = default);

    // The current paid period runs to its end; no further charges.
    Task CancelRenewalAsync(string gatewaySubscriptionId, CancellationToken ct = default);

    // Stops the subscription now (admin cancel), so the owner isn't charged for Premium they lost.
    Task CancelImmediatelyAsync(string gatewaySubscriptionId, CancellationToken ct = default);

    bool IsAuthenticWebhook(string payload, string signature);

    // Null when the payload isn't a well-formed event of the expected shape.
    PaymentEvent? ParseWebhook(string payload);
}
