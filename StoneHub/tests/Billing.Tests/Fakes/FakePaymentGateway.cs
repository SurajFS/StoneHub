using Billing.Application.Payments;

namespace Billing.Tests.Fakes;

// Stands in for Razorpay in handler tests: signature check and parsing are scripted per test,
// and cancellations are recorded so tests can assert the gateway was (or wasn't) told.
public sealed class FakePaymentGateway : IPaymentGateway
{
    public bool IsAvailable { get; init; } = true;

    public bool WebhookIsAuthentic { get; init; } = true;

    public PaymentEvent? ParsedEvent { get; init; }

    public List<string> CancelledImmediately { get; } = [];

    public List<string> CancelledRenewals { get; } = [];

    public Task<GatewayCheckout> CreateRecurringCheckoutAsync(Guid userId, CancellationToken ct = default) =>
        throw new NotSupportedException("Checkout isn't exercised by these tests.");

    public Task CancelRenewalAsync(string gatewaySubscriptionId, CancellationToken ct = default)
    {
        CancelledRenewals.Add(gatewaySubscriptionId);
        return Task.CompletedTask;
    }

    public Task CancelImmediatelyAsync(string gatewaySubscriptionId, CancellationToken ct = default)
    {
        CancelledImmediately.Add(gatewaySubscriptionId);
        return Task.CompletedTask;
    }

    public bool IsAuthenticWebhook(string payload, string signature) => WebhookIsAuthentic;

    public PaymentEvent? ParseWebhook(string payload) => ParsedEvent;
}
