using Billing.Domain;
using Xunit;

namespace Billing.Tests;

// Gateway charges are the only way money becomes a plan. Webhooks arrive late, twice, and out of
// order, and owners switch plans by resubscribing, so every path that grants, keeps or changes a
// plan gets a test.
public sealed class SubscriptionChargeTests
{
    private const string GatewayId = "sub_first";
    private const string CheckoutId = "sub_checkout";
    private const string CheckoutUrl = "https://rzp.io/i/checkout";

    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(SubscriptionPlan.Basic)]
    [InlineData(SubscriptionPlan.Premium)]
    public void FirstCharge_GrantsPlanUntilPeriodEndPlusGrace_RaisesActivatedWithPlan(SubscriptionPlan plan)
    {
        var subscription = Checkout(plan);
        var periodEnd = Now.AddDays(30);

        var applied = subscription.ApplyGatewayCharge(CheckoutId, plan, periodEnd);

        var expiry = periodEnd + Subscription.RenewalGracePeriod;
        Assert.True(applied);
        Assert.Equal(plan, subscription.Plan);
        Assert.True(subscription.IsPaidActive);
        Assert.True(subscription.WillRenew);
        Assert.True(subscription.HasEverPaid);
        Assert.Equal(expiry, subscription.ExpiryDate);
        var activated = Assert.IsType<SubscriptionActivatedEvent>(Assert.Single(subscription.DomainEvents));
        Assert.Equal(plan, activated.Plan);
        Assert.Equal(expiry, activated.ExpiryDate);
    }

    [Fact]
    public void Renewal_ExtendsExpiry_RaisesNothing()
    {
        var subscription = Paid(SubscriptionPlan.Basic, Now.AddDays(30));

        Assert.True(subscription.ApplyGatewayCharge(GatewayId, SubscriptionPlan.Basic, Now.AddDays(60)));

        Assert.Equal(Now.AddDays(60) + Subscription.RenewalGracePeriod, subscription.ExpiryDate);
        Assert.Empty(subscription.DomainEvents);
    }

    [Fact]
    public void OlderOrDuplicateChargeOfCurrentSubscription_IsStale_ChangesNothing()
    {
        var subscription = Paid(SubscriptionPlan.Basic, Now.AddDays(60));

        Assert.False(subscription.ApplyGatewayCharge(GatewayId, SubscriptionPlan.Basic, Now.AddDays(30)));
        Assert.False(subscription.ApplyGatewayCharge(GatewayId, SubscriptionPlan.Basic, Now.AddDays(60)));

        Assert.Equal(Now.AddDays(60) + Subscription.RenewalGracePeriod, subscription.ExpiryDate);
        Assert.Empty(subscription.DomainEvents);
    }

    [Fact]
    public void UpgradeByResubscribing_BasicThenPremiumCheckoutCharged_SwitchesNow_RaisesPlanChanged()
    {
        var subscription = Paid(SubscriptionPlan.Basic, Now.AddDays(20));
        subscription.StopRenewal();
        subscription.StartCheckout(SubscriptionPlan.Premium, CheckoutId, CheckoutUrl, Now.AddHours(24));

        Assert.True(subscription.ApplyGatewayCharge(CheckoutId, SubscriptionPlan.Premium, Now.AddDays(30)));

        Assert.Equal(SubscriptionPlan.Premium, subscription.Plan);
        Assert.Equal(CheckoutId, subscription.GatewaySubscriptionId);
        Assert.True(subscription.WillRenew);
        var changed = Assert.IsType<SubscriptionPlanChangedEvent>(Assert.Single(subscription.DomainEvents));
        Assert.Equal(SubscriptionPlan.Premium, changed.Plan);
    }

    // Downgrading mid-period: the new Basic period may end before the old Premium one would
    // have, but the owner chose to switch now, so Basic replaces Premium (shorter expiry too).
    [Fact]
    public void DowngradeByResubscribing_BasicCheckoutCharged_ReplacesPremiumEvenIfShorter()
    {
        var subscription = Paid(SubscriptionPlan.Premium, Now.AddDays(40));
        subscription.StopRenewal();
        subscription.StartCheckout(SubscriptionPlan.Basic, CheckoutId, CheckoutUrl, Now.AddHours(24));

        Assert.True(subscription.ApplyGatewayCharge(CheckoutId, SubscriptionPlan.Basic, Now.AddDays(30)));

        Assert.Equal(SubscriptionPlan.Basic, subscription.Plan);
        Assert.Equal(Now.AddDays(30) + Subscription.RenewalGracePeriod, subscription.ExpiryDate);
        Assert.IsType<SubscriptionPlanChangedEvent>(Assert.Single(subscription.DomainEvents));
    }

    [Fact]
    public void LateChargeFromReplacedSubscription_IsStale_DoesNotSwitchBack()
    {
        var subscription = Paid(SubscriptionPlan.Basic, Now.AddDays(20));
        subscription.StopRenewal();
        subscription.StartCheckout(SubscriptionPlan.Premium, CheckoutId, CheckoutUrl, Now.AddHours(24));
        subscription.ApplyGatewayCharge(CheckoutId, SubscriptionPlan.Premium, Now.AddDays(30));
        subscription.ClearDomainEvents();

        Assert.False(subscription.ApplyGatewayCharge(GatewayId, SubscriptionPlan.Basic, Now.AddDays(20)));

        Assert.Equal(SubscriptionPlan.Premium, subscription.Plan);
        Assert.Equal(CheckoutId, subscription.GatewaySubscriptionId);
        Assert.Empty(subscription.DomainEvents);
    }

    // Decision 2026-10-28: a charge is money taken, so a late charge after an admin cancel grants
    // the plan again rather than being dropped.
    [Fact]
    public void ChargeAfterAdminCancel_GrantsPlanAgain()
    {
        var subscription = Paid(SubscriptionPlan.Basic, Now.AddDays(30));
        subscription.Cancel();
        subscription.ClearDomainEvents();

        Assert.True(subscription.ApplyGatewayCharge(GatewayId, SubscriptionPlan.Basic, Now.AddDays(30)));

        Assert.True(subscription.IsPaidActive);
        Assert.IsType<SubscriptionActivatedEvent>(Assert.Single(subscription.DomainEvents));
    }

    [Fact]
    public void ChargeAfterExpiry_FromUnknownSubscription_StillGrantsPlan()
    {
        var subscription = Paid(SubscriptionPlan.Basic, Now.AddDays(-5));
        subscription.ExpireIfDue(Now);
        subscription.ClearDomainEvents();

        Assert.True(subscription.ApplyGatewayCharge("sub_unknown", SubscriptionPlan.Premium, Now.AddDays(30)));

        Assert.Equal(SubscriptionPlan.Premium, subscription.Plan);
        Assert.Equal("sub_unknown", subscription.GatewaySubscriptionId);
    }

    [Fact]
    public void ChargeForFree_Throws()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            subscription.ApplyGatewayCharge(GatewayId, SubscriptionPlan.Free, Now.AddDays(30)));
    }

    [Fact]
    public void AdminActivationAfterCharge_ToOtherPlan_RaisesPlanChanged()
    {
        var subscription = Paid(SubscriptionPlan.Basic, Now.AddDays(30));

        subscription.ActivatePlan(SubscriptionPlan.Premium, Now.AddDays(90), "upgrade paid offline");

        Assert.Equal(SubscriptionPlan.Premium, subscription.Plan);
        Assert.IsType<SubscriptionPlanChangedEvent>(Assert.Single(subscription.DomainEvents));
    }

    [Fact]
    public void ActivatePlan_Free_Fails_ChangesNothing()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());

        Assert.True(subscription.ActivatePlan(SubscriptionPlan.Free, Now.AddDays(30), null).IsFailure);
        Assert.Empty(subscription.DomainEvents);
    }

    private static Subscription Checkout(SubscriptionPlan plan)
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.StartCheckout(plan, CheckoutId, CheckoutUrl, Now.AddHours(24));
        return subscription;
    }

    // An owner already on a renewing paid plan linked to GatewayId, with events cleared.
    private static Subscription Paid(SubscriptionPlan plan, DateTimeOffset periodEnd)
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ApplyGatewayCharge(GatewayId, plan, periodEnd);
        subscription.ClearDomainEvents();
        return subscription;
    }
}
