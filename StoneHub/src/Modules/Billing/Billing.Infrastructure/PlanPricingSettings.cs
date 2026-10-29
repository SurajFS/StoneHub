namespace Billing.Infrastructure;

// Bound from "Billing:Pricing". Display prices in paise; they must match the Razorpay plans and
// the launch Offer (the dashboard decides what is actually charged).
public sealed class PlanPricingSettings
{
    public const string SectionName = "Billing:Pricing";

    public int BasicMonthlyPricePaise { get; set; }
    public int PremiumMonthlyPricePaise { get; set; }

    // The launch Offer's discount on the first month, e.g. 25. 0 = no launch offer shown.
    public int LaunchDiscountPercent { get; set; }
}
