using Billing.Domain;
using SharedKernel;
using Xunit;

namespace Billing.Tests;

// Gateway checkout rules: when an owner may start one, and when an unpaid one is reused. These
// decide whether an owner can pay at all and whether they could end up paying twice.
public sealed class SubscriptionGatewayTests
{
    private const string GatewayId = "sub_first";
    private const string CheckoutId = "sub_checkout";
    private const string CheckoutUrl = "https://rzp.io/i/checkout";

    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(SubscriptionPlan.Basic)]
    [InlineData(SubscriptionPlan.Premium)]
    public void StartCheckout_WhenFree_StoresCheckoutForThatPlan_LeavesPlanFree(SubscriptionPlan plan)
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());

        var result = subscription.StartCheckout(plan, CheckoutId, CheckoutUrl, Now.AddHours(24));

        Assert.True(result.IsSuccess);
        Assert.Equal(CheckoutId, subscription.CheckoutGatewaySubscriptionId);
        Assert.Equal(plan, subscription.CheckoutPlan);
        Assert.Null(subscription.GatewaySubscriptionId);
        Assert.Equal(SubscriptionPlan.Free, subscription.Plan);
    }

    [Fact]
    public void StartCheckout_ForFree_Fails()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());

        var result = subscription.StartCheckout(SubscriptionPlan.Free, CheckoutId, CheckoutUrl, Now.AddHours(24));

        Assert.True(result.IsFailure);
        Assert.Null(subscription.CheckoutGatewaySubscriptionId);
    }

    [Theory]
    [InlineData(SubscriptionPlan.Basic)]
    [InlineData(SubscriptionPlan.Premium)]
    public void StartCheckout_WhileAPaidPlanRenews_ReturnsConflict(SubscriptionPlan target)
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ApplyGatewayCharge(GatewayId, SubscriptionPlan.Basic, Now.AddDays(30));

        var result = subscription.StartCheckout(target, CheckoutId, CheckoutUrl, Now.AddHours(24));

        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Null(subscription.CheckoutGatewaySubscriptionId);
    }

    [Fact]
    public void StartCheckout_AfterRenewalStopped_IsAllowed_PlanKeepsRunning()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ApplyGatewayCharge(GatewayId, SubscriptionPlan.Basic, Now.AddDays(30));
        subscription.StopRenewal();

        var result = subscription.StartCheckout(SubscriptionPlan.Premium, CheckoutId, CheckoutUrl, Now.AddHours(24));

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionPlan.Basic, subscription.Plan);
        Assert.Equal(GatewayId, subscription.GatewaySubscriptionId);
    }

    [Fact]
    public void StartCheckout_WhileAdminActivated_IsAllowed()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ActivatePlan(SubscriptionPlan.Basic, Now.AddDays(30), "launch offer");

        Assert.True(subscription.StartCheckout(SubscriptionPlan.Premium, CheckoutId, CheckoutUrl, Now.AddHours(24)).IsSuccess);
    }

    [Fact]
    public void ReusableCheckoutUrl_SamePlanBeforeExpiry_ReturnsUrl()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.StartCheckout(SubscriptionPlan.Basic, CheckoutId, CheckoutUrl, Now.AddHours(24));

        Assert.Equal(CheckoutUrl, subscription.ReusableCheckoutUrl(SubscriptionPlan.Basic, Now));
    }

    [Fact]
    public void ReusableCheckoutUrl_OtherPlan_OrExpired_ReturnsNull()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.StartCheckout(SubscriptionPlan.Basic, CheckoutId, CheckoutUrl, Now.AddHours(24));

        Assert.Null(subscription.ReusableCheckoutUrl(SubscriptionPlan.Premium, Now));
        Assert.Null(subscription.ReusableCheckoutUrl(SubscriptionPlan.Basic, Now.AddHours(24)));
    }

    [Fact]
    public void ReusableCheckoutUrl_OnceCharged_ReturnsNull_AndCheckoutIsCleared()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.StartCheckout(SubscriptionPlan.Basic, CheckoutId, CheckoutUrl, Now.AddHours(24));

        subscription.ApplyGatewayCharge(CheckoutId, SubscriptionPlan.Basic, Now.AddDays(30));

        Assert.Null(subscription.ReusableCheckoutUrl(SubscriptionPlan.Basic, Now));
        Assert.Null(subscription.CheckoutGatewaySubscriptionId);
        Assert.Null(subscription.CheckoutPlan);
        Assert.Equal(CheckoutId, subscription.GatewaySubscriptionId);
    }

    [Fact]
    public void StopRenewal_KeepsPlanUntilExpiry_ThenSweepDowngrades()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ApplyGatewayCharge(GatewayId, SubscriptionPlan.Basic, Now.AddDays(30));

        subscription.StopRenewal();

        Assert.False(subscription.WillRenew);
        Assert.True(subscription.IsPaidActive);
        Assert.False(subscription.ExpireIfDue(Now.AddDays(31)));
        Assert.True(subscription.ExpireIfDue(Now.AddDays(30) + Subscription.RenewalGracePeriod));
        Assert.Equal(SubscriptionPlan.Free, subscription.Plan);
    }

    [Fact]
    public void Cancel_WhenBasic_RevertsToFree_ClearsWillRenew()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ApplyGatewayCharge(GatewayId, SubscriptionPlan.Basic, Now.AddDays(30));
        subscription.ClearDomainEvents();

        subscription.Cancel();

        Assert.Equal(SubscriptionPlan.Free, subscription.Plan);
        Assert.False(subscription.WillRenew);
        Assert.IsType<SubscriptionCancelledEvent>(Assert.Single(subscription.DomainEvents));
    }

    [Fact]
    public void ActivatePlan_AfterGatewayCharge_StopsRenewing()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ApplyGatewayCharge(GatewayId, SubscriptionPlan.Premium, Now.AddDays(30));

        subscription.ActivatePlan(SubscriptionPlan.Premium, Now.AddDays(365), "annual, paid offline");

        Assert.False(subscription.WillRenew);
        Assert.Equal(Now.AddDays(365), subscription.ExpiryDate);
    }
}
