using Billing.Domain;
using SharedKernel;
using Xunit;

namespace Billing.Tests;

// Gateway-driven transitions: checkout, charges (first payment and renewals), and renewal
// ending. These decide whether a paying seller has Premium, so every guard gets a test.
public sealed class SubscriptionGatewayTests
{
    private const string GatewayId = "sub_first";
    private const string OtherGatewayId = "sub_second";
    private const string CheckoutUrl = "https://rzp.io/i/checkout";

    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void StartCheckout_WhenFree_LinksGatewayAndStoresCheckout()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());

        var result = subscription.StartCheckout(GatewayId, CheckoutUrl, Now.AddHours(24));

        Assert.True(result.IsSuccess);
        Assert.Equal(GatewayId, subscription.GatewaySubscriptionId);
        Assert.Equal(CheckoutUrl, subscription.CheckoutUrl);
        Assert.False(subscription.WillRenew);
        Assert.Equal(SubscriptionPlan.Free, subscription.Plan);
    }

    [Fact]
    public void StartCheckout_WhenPremiumActive_ReturnsConflict_LeavesStateUntouched()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ActivatePremium(Now.AddDays(30), "offline");

        var result = subscription.StartCheckout(GatewayId, CheckoutUrl, Now.AddHours(24));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Null(subscription.GatewaySubscriptionId);
        Assert.Null(subscription.CheckoutUrl);
    }

    [Fact]
    public void ReusableCheckoutUrl_BeforeExpiry_ReturnsUrl_AfterExpiry_ReturnsNull()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.StartCheckout(GatewayId, CheckoutUrl, Now.AddHours(24));

        Assert.Equal(CheckoutUrl, subscription.ReusableCheckoutUrl(Now));
        Assert.Null(subscription.ReusableCheckoutUrl(Now.AddHours(24)));
    }

    [Fact]
    public void ReusableCheckoutUrl_OnceCharged_ReturnsNull()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.StartCheckout(GatewayId, CheckoutUrl, Now.AddHours(24));

        subscription.ApplyGatewayCharge(GatewayId, Now.AddDays(30));

        Assert.Null(subscription.ReusableCheckoutUrl(Now));
        Assert.Null(subscription.CheckoutExpiresAt);
    }

    [Fact]
    public void ApplyGatewayCharge_WhenFree_GrantsPremiumUntilPeriodEndPlusGrace_RaisesActivated()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        var periodEnd = Now.AddDays(30);

        subscription.ApplyGatewayCharge(GatewayId, periodEnd);

        var expectedExpiry = periodEnd + Subscription.RenewalGracePeriod;
        Assert.True(subscription.IsPremiumActive);
        Assert.True(subscription.WillRenew);
        Assert.Equal(GatewayId, subscription.GatewaySubscriptionId);
        Assert.Equal(expectedExpiry, subscription.ExpiryDate);
        var activated = Assert.IsType<SubscriptionActivatedEvent>(Assert.Single(subscription.DomainEvents));
        Assert.Equal(expectedExpiry, activated.ExpiryDate);
    }

    [Fact]
    public void ApplyGatewayCharge_WhenRenewing_ExtendsExpiry_RaisesNothing()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ApplyGatewayCharge(GatewayId, Now.AddDays(30));
        subscription.ClearDomainEvents();

        subscription.ApplyGatewayCharge(GatewayId, Now.AddDays(60));

        Assert.Equal(Now.AddDays(60) + Subscription.RenewalGracePeriod, subscription.ExpiryDate);
        Assert.Empty(subscription.DomainEvents);
    }

    [Fact]
    public void ApplyGatewayCharge_OlderPeriodArrivingLate_NeverShortensExpiry()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ApplyGatewayCharge(GatewayId, Now.AddDays(60));

        subscription.ApplyGatewayCharge(GatewayId, Now.AddDays(30));

        Assert.Equal(Now.AddDays(60) + Subscription.RenewalGracePeriod, subscription.ExpiryDate);
    }

    [Fact]
    public void ApplyGatewayCharge_FromDifferentGatewaySubscription_RelinksToIt()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.StartCheckout(GatewayId, CheckoutUrl, Now.AddHours(24));

        subscription.ApplyGatewayCharge(OtherGatewayId, Now.AddDays(30));

        Assert.Equal(OtherGatewayId, subscription.GatewaySubscriptionId);
    }

    [Fact]
    public void ApplyGatewayCharge_AfterAdminActivation_ClearsNotes_KeepsLaterAdminExpiry()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ActivatePremium(Now.AddDays(90), "offline");
        subscription.ClearDomainEvents();

        subscription.ApplyGatewayCharge(GatewayId, Now.AddDays(30));

        Assert.Equal(Now.AddDays(90), subscription.ExpiryDate);
        Assert.Null(subscription.PaymentNotes);
        Assert.True(subscription.WillRenew);
        Assert.Empty(subscription.DomainEvents);
    }

    // Decision 2026-10-28: a charge is money taken, so a late charge webhook after an admin
    // cancel grants Premium again rather than being dropped.
    [Fact]
    public void ApplyGatewayCharge_AfterAdminCancel_GrantsPremiumAgain()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ApplyGatewayCharge(GatewayId, Now.AddDays(30));
        subscription.Cancel();
        subscription.ClearDomainEvents();

        subscription.ApplyGatewayCharge(GatewayId, Now.AddDays(30));

        Assert.True(subscription.IsPremiumActive);
        Assert.IsType<SubscriptionActivatedEvent>(Assert.Single(subscription.DomainEvents));
    }

    [Fact]
    public void StopRenewal_KeepsPremiumUntilExpiry_ThenSweepDowngrades()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ApplyGatewayCharge(GatewayId, Now.AddDays(30));

        subscription.StopRenewal();

        Assert.False(subscription.WillRenew);
        Assert.True(subscription.IsPremiumActive);
        Assert.False(subscription.ExpireIfDue(Now.AddDays(31)));
        Assert.True(subscription.ExpireIfDue(Now.AddDays(30) + Subscription.RenewalGracePeriod));
    }

    [Fact]
    public void Cancel_WhenRenewing_ClearsWillRenew()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ApplyGatewayCharge(GatewayId, Now.AddDays(30));

        subscription.Cancel();

        Assert.False(subscription.WillRenew);
        Assert.False(subscription.IsPremiumActive);
    }

    [Fact]
    public void ActivatePremium_AfterGatewayCharge_StopsRenewing()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ApplyGatewayCharge(GatewayId, Now.AddDays(30));

        subscription.ActivatePremium(Now.AddDays(365), "annual, paid offline");

        Assert.False(subscription.WillRenew);
        Assert.Equal(Now.AddDays(365), subscription.ExpiryDate);
    }
}
