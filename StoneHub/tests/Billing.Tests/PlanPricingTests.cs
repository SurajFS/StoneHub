using Billing.Application.Subscriptions;
using Billing.Domain;
using Billing.Infrastructure;
using Billing.Infrastructure.Payments;
using Billing.Tests.Fakes;
using Microsoft.Extensions.Options;
using Xunit;

namespace Billing.Tests;

// Prices are what owners are told they'll pay, so the launch maths and who sees the launch price
// are tested. (What is actually charged comes from the Razorpay plan and Offer.)
public sealed class PlanPricingTests
{
    private static readonly PlanPricingSettings Prices = new()
    {
        BasicMonthlyPricePaise = 19900,
        PremiumMonthlyPricePaise = 39900,
        LaunchDiscountPercent = 25
    };

    [Theory]
    [InlineData(SubscriptionPlan.Basic, 19900, 14925)]
    [InlineData(SubscriptionPlan.Premium, 39900, 29925)]
    public void WithOffersConfigured_LaunchPriceIs25PercentOff(SubscriptionPlan plan, int monthly, int launch)
    {
        var pricing = Pricing(basicOffer: "offer_b", premiumOffer: "offer_p");

        Assert.Equal(monthly, pricing.MonthlyPricePaise(plan));
        Assert.Equal(launch, pricing.LaunchPricePaise(plan));
    }

    [Fact]
    public void WithoutAnOfferForThePlan_NoLaunchPrice()
    {
        var pricing = Pricing(basicOffer: "offer_b", premiumOffer: "");

        Assert.NotNull(pricing.LaunchPricePaise(SubscriptionPlan.Basic));
        Assert.Null(pricing.LaunchPricePaise(SubscriptionPlan.Premium));
    }

    [Fact]
    public void WithZeroDiscount_NoLaunchPrice()
    {
        var pricing = new PlanPricing(
            Options.Create(new PlanPricingSettings { BasicMonthlyPricePaise = 19900, LaunchDiscountPercent = 0 }),
            Options.Create(new RazorpaySettings { BasicLaunchOfferId = "offer_b" }));

        Assert.Null(pricing.LaunchPricePaise(SubscriptionPlan.Basic));
    }

    [Fact]
    public async Task Plans_ForFirstTimeOwner_IncludeLaunchPrice_CheapestFirst()
    {
        var directory = new FakeBillingDirectory();
        var userId = directory.AddUser("Seller", "Free");

        var plans = await new GetPlansQueryHandler(directory, Pricing("offer_b", "offer_p"))
            .Handle(new GetPlansQuery(userId), CancellationToken.None);

        Assert.Collection(plans,
            basic => Assert.Equal(("Basic", 19900, (int?)14925), (basic.Plan, basic.MonthlyPricePaise, basic.FirstMonthPricePaise)),
            premium => Assert.Equal(("Premium", 39900, (int?)29925), (premium.Plan, premium.MonthlyPricePaise, premium.FirstMonthPricePaise)));
    }

    [Fact]
    public async Task Plans_ForOwnerWhoPaidBefore_HaveNoLaunchPrice()
    {
        var directory = new FakeBillingDirectory();
        var userId = directory.AddUser("Seller", "Free", "Expired", hasEverPaid: true);

        var plans = await new GetPlansQueryHandler(directory, Pricing("offer_b", "offer_p"))
            .Handle(new GetPlansQuery(userId), CancellationToken.None);

        Assert.All(plans, plan => Assert.Null(plan.FirstMonthPricePaise));
    }

    [Theory]
    [InlineData("plan_b", SubscriptionPlan.Basic)]
    [InlineData("plan_p", SubscriptionPlan.Premium)]
    [InlineData("plan_other", null)]
    [InlineData("", null)]
    public void RazorpaySettings_PlanFor_MapsOnlyOurPlans(string gatewayPlanId, SubscriptionPlan? expected)
    {
        var settings = new RazorpaySettings { BasicPlanId = "plan_b", PremiumPlanId = "plan_p" };

        Assert.Equal(expected, settings.PlanFor(gatewayPlanId));
    }

    [Fact]
    public void RazorpaySettings_NotConfiguredWithoutBothPlanIds()
    {
        var settings = new RazorpaySettings
        {
            ApiBaseUrl = "https://api.razorpay.com/v1/", KeyId = "k", KeySecret = "s", WebhookSecret = "w",
            BasicPlanId = "plan_b", TotalBillingCycles = 100, CheckoutLinkHours = 24
        };

        Assert.False(settings.IsConfigured);
        settings.PremiumPlanId = "plan_p";
        Assert.True(settings.IsConfigured);
    }

    private static PlanPricing Pricing(string basicOffer, string premiumOffer) =>
        new(Options.Create(Prices), Options.Create(new RazorpaySettings
        {
            BasicLaunchOfferId = basicOffer,
            PremiumLaunchOfferId = premiumOffer
        }));
}
