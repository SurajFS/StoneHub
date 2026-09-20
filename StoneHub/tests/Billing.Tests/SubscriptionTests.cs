using Billing.Domain;
using Xunit;

namespace Billing.Tests;

// Subscription state machine: Free-by-default, admin-triggered Premium activation, cancel,
// and the automatic-downgrade expiry sweep. This is the riskiest logic in the module — it
// directly gates paid-feature access — so every transition and guard gets a test.
public sealed class SubscriptionTests
{
    [Fact]
    public void CreateFree_DefaultsToFreeActiveWithNoExpiry()
    {
        var userId = Guid.NewGuid();

        var subscription = Subscription.CreateFree(userId);

        Assert.Equal(userId, subscription.UserId);
        Assert.Equal(SubscriptionPlan.Free, subscription.Plan);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Null(subscription.ExpiryDate);
        Assert.Empty(subscription.DomainEvents);
    }

    [Fact]
    public void ActivatePremium_SetsPlanStatusExpiryAndNotes_RaisesEvent()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        var expiry = DateTimeOffset.UtcNow.AddDays(30);

        subscription.ActivatePremium(expiry, "Manual test activation");

        Assert.Equal(SubscriptionPlan.Premium, subscription.Plan);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Equal(expiry, subscription.ExpiryDate);
        Assert.Equal("Manual test activation", subscription.PaymentNotes);
        var raised = Assert.Single(subscription.DomainEvents);
        var activated = Assert.IsType<SubscriptionActivatedEvent>(raised);
        Assert.Equal(subscription.UserId, activated.UserId);
        Assert.Equal(expiry, activated.ExpiryDate);
    }

    [Fact]
    public void Cancel_WhenPremium_RevertsToFreeCancelled_ClearsExpiryAndNotes_RaisesEvent()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ActivatePremium(DateTimeOffset.UtcNow.AddDays(30), "paid");
        subscription.ClearDomainEvents();

        subscription.Cancel();

        Assert.Equal(SubscriptionPlan.Free, subscription.Plan);
        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
        Assert.Null(subscription.ExpiryDate);
        Assert.Null(subscription.PaymentNotes);
        var raised = Assert.Single(subscription.DomainEvents);
        Assert.IsType<SubscriptionCancelledEvent>(raised);
    }

    [Fact]
    public void Cancel_WhenAlreadyFree_IsNoOp_RaisesNothing()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());

        subscription.Cancel();

        Assert.Equal(SubscriptionPlan.Free, subscription.Plan);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Empty(subscription.DomainEvents);
    }

    [Fact]
    public void ExpireIfDue_WhenFreePlan_ReturnsFalse_RaisesNothing()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());

        var changed = subscription.ExpireIfDue(DateTimeOffset.UtcNow);

        Assert.False(changed);
        Assert.Empty(subscription.DomainEvents);
    }

    [Fact]
    public void ExpireIfDue_WhenPremiumNotYetExpired_ReturnsFalse_StaysPremium()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ActivatePremium(DateTimeOffset.UtcNow.AddDays(30), null);
        subscription.ClearDomainEvents();

        var changed = subscription.ExpireIfDue(DateTimeOffset.UtcNow);

        Assert.False(changed);
        Assert.Equal(SubscriptionPlan.Premium, subscription.Plan);
        Assert.Empty(subscription.DomainEvents);
    }

    [Fact]
    public void ExpireIfDue_WhenPremiumPastExpiry_DowngradesToFreeExpired_RaisesEvent()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        var expiry = DateTimeOffset.UtcNow.AddSeconds(1);
        subscription.ActivatePremium(expiry, "paid");
        subscription.ClearDomainEvents();

        var changed = subscription.ExpireIfDue(expiry.AddMinutes(1));

        Assert.True(changed);
        Assert.Equal(SubscriptionPlan.Free, subscription.Plan);
        Assert.Equal(SubscriptionStatus.Expired, subscription.Status);
        Assert.Null(subscription.PaymentNotes);
        var raised = Assert.Single(subscription.DomainEvents);
        var expired = Assert.IsType<SubscriptionExpiredEvent>(raised);
        Assert.Equal(subscription.UserId, expired.UserId);
    }

    [Fact]
    public void ExpireIfDue_WhenAlreadyCancelled_ReturnsFalse()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ActivatePremium(DateTimeOffset.UtcNow.AddSeconds(1), null);
        subscription.Cancel();
        subscription.ClearDomainEvents();

        var changed = subscription.ExpireIfDue(DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.False(changed);
        Assert.Empty(subscription.DomainEvents);
    }
}
