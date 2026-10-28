using Billing.Application.Payments;
using Billing.Domain;
using Billing.Tests.Fakes;
using SharedKernel;
using Xunit;

namespace Billing.Tests;

// The webhook is the only path by which money turns into Premium, and gateways deliver at
// least once and out of order, so authenticity, idempotency and matching all get tests.
public sealed class HandlePaymentWebhookCommandHandlerTests
{
    private const string EventId = "evt_1";
    private const string GatewayId = "sub_first";
    private const string OtherGatewayId = "sub_second";
    private const string ChargedType = "subscription.charged";
    private const string CancelledType = "subscription.cancelled";

    private static readonly DateTimeOffset PeriodEnd = DateTimeOffset.UtcNow.AddDays(30);

    private readonly InMemoryBillingStore _store = new();

    [Fact]
    public async Task Handle_InvalidSignature_ReturnsUnauthorized_SavesNothing()
    {
        var gateway = new FakePaymentGateway { WebhookIsAuthentic = false, ParsedEvent = Charge(GatewayId, null) };

        var result = await HandleAsync(gateway);

        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
        Assert.Equal(0, _store.SaveCount);
    }

    [Fact]
    public async Task Handle_UnreadablePayload_ReturnsFailure_SavesNothing()
    {
        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = null });

        Assert.Equal(ErrorType.Failure, result.ErrorType);
        Assert.Equal(0, _store.SaveCount);
    }

    [Fact]
    public async Task Handle_Charge_ForLinkedSubscription_GrantsPremium_RecordsEvent()
    {
        var subscription = LinkedSubscription(GatewayId);

        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = Charge(GatewayId, null) });

        Assert.Equal(PaymentWebhookOutcome.Applied, result.Value);
        Assert.True(subscription.IsPremiumActive);
        var recorded = Assert.Single(_store.SavedEvents);
        Assert.Equal(EventId, recorded.Id);
        Assert.Equal(ChargedType, recorded.EventType);
    }

    [Fact]
    public async Task Handle_SameEventDeliveredTwice_SecondIsDuplicate_AppliedOnce()
    {
        var subscription = LinkedSubscription(GatewayId);
        var gateway = new FakePaymentGateway { ParsedEvent = Charge(GatewayId, null) };
        await HandleAsync(gateway);
        subscription.ClearDomainEvents();

        var result = await HandleAsync(gateway);

        Assert.Equal(PaymentWebhookOutcome.Duplicate, result.Value);
        Assert.Empty(subscription.DomainEvents);
        Assert.Single(_store.SavedEvents);
    }

    [Fact]
    public async Task Handle_Charge_WhenGatewayIdReplaced_FallsBackToCheckoutUserId()
    {
        var subscription = LinkedSubscription(OtherGatewayId);

        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = Charge(GatewayId, subscription.UserId) });

        Assert.Equal(PaymentWebhookOutcome.Applied, result.Value);
        Assert.True(subscription.IsPremiumActive);
        Assert.Equal(GatewayId, subscription.GatewaySubscriptionId);
    }

    [Fact]
    public async Task Handle_Charge_ForUnknownSubscription_IsAcknowledgedAndRecorded()
    {
        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = Charge(GatewayId, Guid.NewGuid()) });

        Assert.Equal(PaymentWebhookOutcome.UnknownSubscription, result.Value);
        Assert.Single(_store.SavedEvents);
    }

    [Fact]
    public async Task Handle_ChargeWithoutPeriodEnd_IsIgnored_GrantsNothing()
    {
        var subscription = LinkedSubscription(GatewayId);
        var activation = new PaymentEvent("subscription.activated", PaymentEventKind.Charged, GatewayId, null, null);

        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = activation });

        Assert.Equal(PaymentWebhookOutcome.Ignored, result.Value);
        Assert.False(subscription.IsPremiumActive);
        Assert.Single(_store.SavedEvents);
    }

    [Fact]
    public async Task Handle_IgnoredEventKind_ChangesNothing_IsRecorded()
    {
        var subscription = LinkedSubscription(GatewayId);
        var pending = new PaymentEvent("subscription.pending", PaymentEventKind.Ignored, GatewayId, null, PeriodEnd);

        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = pending });

        Assert.Equal(PaymentWebhookOutcome.Ignored, result.Value);
        Assert.False(subscription.IsPremiumActive);
        Assert.Single(_store.SavedEvents);
    }

    [Fact]
    public async Task Handle_RenewalEnded_ForLinkedSubscription_StopsRenewal_KeepsPremium()
    {
        var subscription = LinkedSubscription(GatewayId);
        subscription.ApplyGatewayCharge(GatewayId, PeriodEnd);

        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = RenewalEnded(GatewayId) });

        Assert.Equal(PaymentWebhookOutcome.Applied, result.Value);
        Assert.False(subscription.WillRenew);
        Assert.True(subscription.IsPremiumActive);
    }

    [Fact]
    public async Task Handle_RenewalEnded_ForOlderGatewaySubscription_LeavesCurrentRenewing()
    {
        var subscription = LinkedSubscription(OtherGatewayId);
        subscription.ApplyGatewayCharge(OtherGatewayId, PeriodEnd);

        var result = await HandleAsync(new FakePaymentGateway { ParsedEvent = RenewalEnded(GatewayId) });

        Assert.Equal(PaymentWebhookOutcome.UnknownSubscription, result.Value);
        Assert.True(subscription.WillRenew);
    }

    private Task<Result<PaymentWebhookOutcome>> HandleAsync(FakePaymentGateway gateway) =>
        new HandlePaymentWebhookCommandHandler(gateway, _store, _store)
            .Handle(new HandlePaymentWebhookCommand(EventId, "{}", "signature"), CancellationToken.None);

    private Subscription LinkedSubscription(string gatewayId)
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.StartCheckout(gatewayId, "https://rzp.io/i/checkout", DateTimeOffset.UtcNow.AddHours(24));
        _store.Add(subscription);
        return subscription;
    }

    private static PaymentEvent Charge(string gatewayId, Guid? userId) =>
        new(ChargedType, PaymentEventKind.Charged, gatewayId, userId, PeriodEnd);

    private static PaymentEvent RenewalEnded(string gatewayId) =>
        new(CancelledType, PaymentEventKind.RenewalEnded, gatewayId, null, null);
}
