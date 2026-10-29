using Billing.Application.Payments;
using Billing.Domain;

namespace Billing.Tests.Fakes;

// Stands in for Razorpay in handler tests: signature check and parsing are scripted per test,
// checkouts and cancellations are recorded so tests can assert what the gateway was asked to do.
public sealed class FakePaymentGateway : IPaymentGateway
{
    public const string BasicPlanId = "plan_basic";
    public const string PremiumPlanId = "plan_premium";
    public const string CheckoutUrl = "https://rzp.io/i/fake";

    public bool IsAvailable { get; init; } = true;

    public bool WebhookIsAuthentic { get; init; } = true;

    public PaymentEvent? ParsedEvent { get; init; }

    public List<string> CancelledImmediately { get; } = [];

    public List<string> CancelledRenewals { get; } = [];

    public List<(Guid UserId, SubscriptionPlan Plan, bool WithLaunchOffer)> Checkouts { get; } = [];

    public Task<GatewayCheckout> CreateRecurringCheckoutAsync(
        Guid userId, SubscriptionPlan plan, bool withLaunchOffer, CancellationToken ct = default)
    {
        Checkouts.Add((userId, plan, withLaunchOffer));
        return Task.FromResult(new GatewayCheckout($"sub_checkout_{Checkouts.Count}", CheckoutUrl, DateTimeOffset.UtcNow.AddHours(24)));
    }

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

    public SubscriptionPlan? PlanFor(string gatewayPlanId) => gatewayPlanId switch
    {
        BasicPlanId => SubscriptionPlan.Basic,
        PremiumPlanId => SubscriptionPlan.Premium,
        _ => null
    };
}
