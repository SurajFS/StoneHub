using Billing.Domain;

namespace Billing.Application;

// Prices shown to owners, in paise. Display only: what is actually charged is set by the
// gateway's plan and offer, so these must be kept equal to them (see appsettings "Billing").
public interface IPlanPricing
{
    int MonthlyPricePaise(SubscriptionPlan plan);

    // First-month price for a first-time subscriber; null when no launch offer runs for the plan.
    int? LaunchPricePaise(SubscriptionPlan plan);
}
