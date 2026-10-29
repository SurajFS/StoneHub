using Billing.Application.Payments;
using Billing.Domain;
using Billing.Tests.Fakes;
using SharedKernel;
using Xunit;

namespace Billing.Tests;

// The webhook is the only path by which money turns into a plan, and gateways deliver at least
// once and out of order, so authenticity, idempotency, plan matching and owner matching all get
// tests.
public sealed class HandlePaymentWebhookCommandHandlerTests
{
    private const string EventId = "evt_1";
    private const string CheckoutId = "sub_checkout";
    private const string OtherGatewayId = "sub_other";
    private const string ChargedType = "subscription.charged";
    private const string CancelledType = "subscription.cancelled";

    private static readonly DateTimeOffset PeriodEnd = DateTimeOffset.UtcNow.AddDays(30);

    private readonly InMemoryBillingStore _store = new();

    [Fact]
    public async Task InvalidSignature_ReturnsUnauthorized_SavesNothing()
    {
        var gateway = new FakePaymentGateway { WebhookIsAuthentic = false, ParsedEvent = Charge(CheckoutId, FakePaymentGateway.BasicPlanId) };

        var result = await HandleAsync(gateway);

        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
        Assert.Equal(0, _store.SaveCount);
    }

    [Fact]
    public async Task UnreadablePayload_ReturnsFailure_SavesNothing()
    {
        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = null });

        Assert.Equal(ErrorType.Failure, result.ErrorType);
        Assert.Equal(0, _store.SaveCount);
    }

    [Theory]
    [InlineData(FakePaymentGateway.BasicPlanId, SubscriptionPlan.Basic)]
    [InlineData(FakePaymentGateway.PremiumPlanId, SubscriptionPlan.Premium)]
    public async Task ChargeOfCheckout_GrantsTheChargedPlan_RecordsEvent(string gatewayPlanId, SubscriptionPlan expected)
    {
        var subscription = WithCheckout(expected);

        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = Charge(CheckoutId, gatewayPlanId) });

        Assert.Equal(PaymentWebhookOutcome.Applied, result.Value);
        Assert.Equal(expected, subscription.Plan);
        Assert.Equal(CheckoutId, subscription.GatewaySubscriptionId);
        var recorded = Assert.Single(_store.SavedEvents);
        Assert.Equal(EventId, recorded.Id);
        Assert.Equal(ChargedType, recorded.EventType);
    }

    [Fact]
    public async Task ChargeForUnknownGatewayPlan_IsUnknownPlan_GrantsNothing_IsRecorded()
    {
        var subscription = WithCheckout(SubscriptionPlan.Basic);

        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = Charge(CheckoutId, "plan_not_ours") });

        Assert.Equal(PaymentWebhookOutcome.UnknownPlan, result.Value);
        Assert.False(subscription.IsPaidActive);
        Assert.Single(_store.SavedEvents);
    }

    [Fact]
    public async Task SameEventDeliveredTwice_SecondIsDuplicate_AppliedOnce()
    {
        var subscription = WithCheckout(SubscriptionPlan.Basic);
        var gateway = new FakePaymentGateway { ParsedEvent = Charge(CheckoutId, FakePaymentGateway.BasicPlanId) };
        await HandleAsync(gateway);
        subscription.ClearDomainEvents();

        var result = await HandleAsync(gateway);

        Assert.Equal(PaymentWebhookOutcome.Duplicate, result.Value);
        Assert.Empty(subscription.DomainEvents);
        Assert.Single(_store.SavedEvents);
    }

    [Fact]
    public async Task ChargeOfUnlinkedSubscription_FallsBackToCheckoutUserId()
    {
        var subscription = WithCheckout(SubscriptionPlan.Basic);

        var result = await HandleAsync(new FakePaymentGateway
        {
            ParsedEvent = Charge(OtherGatewayId, FakePaymentGateway.PremiumPlanId, subscription.UserId)
        });

        Assert.Equal(PaymentWebhookOutcome.Applied, result.Value);
        Assert.Equal(SubscriptionPlan.Premium, subscription.Plan);
        Assert.Equal(OtherGatewayId, subscription.GatewaySubscriptionId);
    }

    [Fact]
    public async Task ChargeForUnknownOwner_IsAcknowledgedAndRecorded()
    {
        var result = await HandleAsync(new FakePaymentGateway
        {
            ParsedEvent = Charge(CheckoutId, FakePaymentGateway.BasicPlanId, Guid.NewGuid())
        });

        Assert.Equal(PaymentWebhookOutcome.UnknownSubscription, result.Value);
        Assert.Single(_store.SavedEvents);
    }

    [Fact]
    public async Task OlderChargeOfCurrentSubscription_IsStale()
    {
        var subscription = WithCheckout(SubscriptionPlan.Basic);
        subscription.ApplyGatewayCharge(CheckoutId, SubscriptionPlan.Basic, PeriodEnd.AddDays(30));

        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = Charge(CheckoutId, FakePaymentGateway.BasicPlanId) });

        Assert.Equal(PaymentWebhookOutcome.Stale, result.Value);
        Assert.Equal(PeriodEnd.AddDays(30) + Subscription.RenewalGracePeriod, subscription.ExpiryDate);
    }

    [Fact]
    public async Task ChargeWithoutPeriodEnd_IsIgnored_GrantsNothing()
    {
        var subscription = WithCheckout(SubscriptionPlan.Basic);
        var activation = new PaymentEvent("subscription.activated", PaymentEventKind.Charged, CheckoutId, null, null, FakePaymentGateway.BasicPlanId);

        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = activation });

        Assert.Equal(PaymentWebhookOutcome.Ignored, result.Value);
        Assert.False(subscription.IsPaidActive);
        Assert.Single(_store.SavedEvents);
    }

    [Fact]
    public async Task IgnoredEventKind_ChangesNothing_IsRecorded()
    {
        var subscription = WithCheckout(SubscriptionPlan.Basic);
        var pending = new PaymentEvent("subscription.pending", PaymentEventKind.Ignored, CheckoutId, null, PeriodEnd, FakePaymentGateway.BasicPlanId);

        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = pending });

        Assert.Equal(PaymentWebhookOutcome.Ignored, result.Value);
        Assert.False(subscription.IsPaidActive);
        Assert.Single(_store.SavedEvents);
    }

    [Fact]
    public async Task RenewalEnded_ForPayingSubscription_StopsRenewal_KeepsPlan()
    {
        var subscription = WithCheckout(SubscriptionPlan.Basic);
        subscription.ApplyGatewayCharge(CheckoutId, SubscriptionPlan.Basic, PeriodEnd);

        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = RenewalEnded(CheckoutId) });

        Assert.Equal(PaymentWebhookOutcome.Applied, result.Value);
        Assert.False(subscription.WillRenew);
        Assert.True(subscription.IsPaidActive);
    }

    // The owner stopped Basic and started a Premium checkout; that unpaid checkout lapsing must
    // not touch the current Basic subscription.
    [Fact]
    public async Task RenewalEnded_ForUnpaidCheckout_IsIgnored_CurrentPlanUntouched()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ApplyGatewayCharge(OtherGatewayId, SubscriptionPlan.Basic, PeriodEnd);
        subscription.StopRenewal();
        subscription.StartCheckout(SubscriptionPlan.Premium, CheckoutId, "https://rzp.io/i/checkout", DateTimeOffset.UtcNow.AddHours(24));
        _store.Add(subscription);

        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = RenewalEnded(CheckoutId) });

        Assert.Equal(PaymentWebhookOutcome.Ignored, result.Value);
        Assert.Equal(SubscriptionPlan.Basic, subscription.Plan);
        Assert.Equal(OtherGatewayId, subscription.GatewaySubscriptionId);
    }

    [Fact]
    public async Task RenewalEnded_ForUnknownSubscription_IsUnknown()
    {
        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = RenewalEnded("sub_nobody") });

        Assert.Equal(PaymentWebhookOutcome.UnknownSubscription, result.Value);
    }

    private Task<Result<PaymentWebhookOutcome>> HandleAsync(FakePaymentGateway gateway) =>
        new HandlePaymentWebhookCommandHandler(gateway, _store, _store)
            .Handle(new HandlePaymentWebhookCommand(EventId, "{}", "signature"), CancellationToken.None);

    private Subscription WithCheckout(SubscriptionPlan plan)
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.StartCheckout(plan, CheckoutId, "https://rzp.io/i/checkout", DateTimeOffset.UtcNow.AddHours(24));
        _store.Add(subscription);
        return subscription;
    }

    private static PaymentEvent Charge(string gatewayId, string gatewayPlanId, Guid? userId = null) =>
        new(ChargedType, PaymentEventKind.Charged, gatewayId, userId, PeriodEnd, gatewayPlanId);

    private static PaymentEvent RenewalEnded(string gatewayId) =>
        new(CancelledType, PaymentEventKind.RenewalEnded, gatewayId, null, null);
}
