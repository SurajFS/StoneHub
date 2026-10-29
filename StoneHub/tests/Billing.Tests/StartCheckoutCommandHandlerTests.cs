using Billing.Application.Subscriptions;
using Billing.Domain;
using Billing.Tests.Fakes;
using SharedKernel;
using Xunit;

namespace Billing.Tests;

// Checkout decides what the owner is asked to pay: which plan, whether the launch discount is
// attached, and that a still-renewing plan can't be paid for twice.
public sealed class StartCheckoutCommandHandlerTests
{
    private readonly InMemoryBillingStore _store = new();
    private readonly FakePaymentGateway _gateway = new();

    [Theory]
    [InlineData(SubscriptionPlan.Basic)]
    [InlineData(SubscriptionPlan.Premium)]
    public async Task FirstTimeOwner_GetsCheckoutForThatPlan_WithLaunchOffer(SubscriptionPlan plan)
    {
        var subscription = Owner();

        var result = await HandleAsync(subscription.UserId, plan);

        Assert.Equal(FakePaymentGateway.CheckoutUrl, result.Value.CheckoutUrl);
        Assert.Equal((subscription.UserId, plan, true), Assert.Single(_gateway.Checkouts));
        Assert.Equal(plan, subscription.CheckoutPlan);
    }

    [Fact]
    public async Task OwnerWhoPaidBefore_GetsNoLaunchOffer()
    {
        var subscription = Owner();
        subscription.ApplyGatewayCharge("sub_old", SubscriptionPlan.Basic, DateTimeOffset.UtcNow.AddDays(-10));
        subscription.ExpireIfDue(DateTimeOffset.UtcNow);

        await HandleAsync(subscription.UserId, SubscriptionPlan.Basic);

        Assert.False(Assert.Single(_gateway.Checkouts).WithLaunchOffer);
    }

    [Fact]
    public async Task WhilePlanRenews_ReturnsConflict_NoCheckoutCreated()
    {
        var subscription = Owner();
        subscription.ApplyGatewayCharge("sub_current", SubscriptionPlan.Basic, DateTimeOffset.UtcNow.AddDays(20));

        var result = await HandleAsync(subscription.UserId, SubscriptionPlan.Premium);

        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Empty(_gateway.Checkouts);
    }

    [Fact]
    public async Task SamePlanTwice_ReusesTheUnpaidCheckout()
    {
        var subscription = Owner();
        await HandleAsync(subscription.UserId, SubscriptionPlan.Basic);

        var second = await HandleAsync(subscription.UserId, SubscriptionPlan.Basic);

        Assert.Equal(FakePaymentGateway.CheckoutUrl, second.Value.CheckoutUrl);
        Assert.Single(_gateway.Checkouts);
    }

    [Fact]
    public async Task OwnerWithoutRow_GetsFreeRowCreated_AndACheckout()
    {
        var userId = Guid.NewGuid();

        var result = await HandleAsync(userId, SubscriptionPlan.Basic);

        Assert.True(result.IsSuccess);
        var created = await _store.GetByUserIdAsync(userId);
        Assert.NotNull(created);
        Assert.Equal(SubscriptionPlan.Free, created.Plan);
        Assert.Equal(SubscriptionPlan.Basic, created.CheckoutPlan);
    }

    [Fact]
    public async Task GatewayNotConfigured_ReturnsUnavailable()
    {
        var gateway = new FakePaymentGateway { IsAvailable = false };

        var result = await new StartCheckoutCommandHandler(gateway, _store)
            .Handle(new StartCheckoutCommand(Owner().UserId, SubscriptionPlan.Basic), CancellationToken.None);

        Assert.Equal(ErrorType.Unavailable, result.ErrorType);
    }

    [Theory]
    [InlineData(SubscriptionPlan.Free, false)]
    [InlineData(SubscriptionPlan.Basic, true)]
    [InlineData(SubscriptionPlan.Premium, true)]
    [InlineData((SubscriptionPlan)99, false)]
    public void Validator_AcceptsOnlyPaidPlans(SubscriptionPlan plan, bool valid)
    {
        var result = new StartCheckoutCommandValidator().Validate(new StartCheckoutCommand(Guid.NewGuid(), plan));

        Assert.Equal(valid, result.IsValid);
    }

    private Task<Result<Billing.Application.Dtos.CheckoutDto>> HandleAsync(Guid userId, SubscriptionPlan plan) =>
        new StartCheckoutCommandHandler(_gateway, _store).Handle(new StartCheckoutCommand(userId, plan), CancellationToken.None);

    private Subscription Owner()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        _store.Add(subscription);
        return subscription;
    }
}
