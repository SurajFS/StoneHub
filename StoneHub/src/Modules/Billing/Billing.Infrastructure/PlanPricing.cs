using Billing.Application;
using Billing.Domain;
using Billing.Infrastructure.Payments;
using Microsoft.Extensions.Options;

namespace Billing.Infrastructure;

// The launch price is only shown for a plan that actually has a Razorpay Offer configured, so the
// app never promises a discount checkout won't apply.
public sealed class PlanPricing(IOptions<PlanPricingSettings> pricing, IOptions<RazorpaySettings> razorpay) : IPlanPricing
{
    private const int WholePercent = 100;

    public int MonthlyPricePaise(SubscriptionPlan plan) => plan switch
    {
        SubscriptionPlan.Basic => pricing.Value.BasicMonthlyPricePaise,
        SubscriptionPlan.Premium => pricing.Value.PremiumMonthlyPricePaise,
        _ => 0
    };

    public int? LaunchPricePaise(SubscriptionPlan plan)
    {
        var discount = pricing.Value.LaunchDiscountPercent;
        if (discount <= 0 || razorpay.Value.LaunchOfferIdFor(plan) is null)
            return null;

        return MonthlyPricePaise(plan) * (WholePercent - discount) / WholePercent;
    }
}
