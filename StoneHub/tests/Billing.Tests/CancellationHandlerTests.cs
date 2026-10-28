using Billing.Application.Subscriptions;
using Billing.Domain;
using Billing.Tests.Fakes;
using SharedKernel;
using Xunit;

namespace Billing.Tests;

// Cancelling must reach the gateway, or the seller keeps being charged for Premium they no
// longer have (admin cancel) or didn't want renewed (owner cancel).
public sealed class CancellationHandlerTests
{
    private const string GatewayId = "sub_first";

    private readonly InMemoryBillingStore _store = new();
    private readonly FakePaymentGateway _gateway = new();

    [Fact]
    public async Task AdminCancel_WhenRenewing_CancelsAtGatewayNow_AndDowngrades()
    {
        var subscription = RenewingSubscription();

        var result = await new CancelSubscriptionCommandHandler(_gateway, _store)
            .Handle(new CancelSubscriptionCommand(subscription.UserId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal([GatewayId], _gateway.CancelledImmediately);
        Assert.False(subscription.IsPremiumActive);
    }

    [Fact]
    public async Task AdminCancel_WhenAdminActivated_DoesNotCallGateway()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ActivatePremium(DateTimeOffset.UtcNow.AddDays(30), "offline");
        _store.Add(subscription);

        await new CancelSubscriptionCommandHandler(_gateway, _store)
            .Handle(new CancelSubscriptionCommand(subscription.UserId), CancellationToken.None);

        Assert.Empty(_gateway.CancelledImmediately);
        Assert.False(subscription.IsPremiumActive);
    }

    [Fact]
    public async Task OwnerCancelRenewal_WhenRenewing_CancelsRenewalAtGateway_KeepsPremium()
    {
        var subscription = RenewingSubscription();

        var result = await new CancelPremiumRenewalCommandHandler(_gateway, _store)
            .Handle(new CancelPremiumRenewalCommand(subscription.UserId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal([GatewayId], _gateway.CancelledRenewals);
        Assert.False(subscription.WillRenew);
        Assert.True(subscription.IsPremiumActive);
    }

    [Fact]
    public async Task OwnerCancelRenewal_WhenNotRenewing_ReturnsConflict_DoesNotCallGateway()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        _store.Add(subscription);

        var result = await new CancelPremiumRenewalCommandHandler(_gateway, _store)
            .Handle(new CancelPremiumRenewalCommand(subscription.UserId), CancellationToken.None);

        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Empty(_gateway.CancelledRenewals);
    }

    [Fact]
    public async Task OwnerCancelRenewal_UnknownUser_ReturnsNotFound()
    {
        var result = await new CancelPremiumRenewalCommandHandler(_gateway, _store)
            .Handle(new CancelPremiumRenewalCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    private Subscription RenewingSubscription()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ApplyGatewayCharge(GatewayId, DateTimeOffset.UtcNow.AddDays(30));
        _store.Add(subscription);
        return subscription;
    }
}
