using Billing.Domain;

namespace Billing.Infrastructure.Payments;

// Razorpay config. Keys and the webhook secret come from user-secrets / the environment
// (Razorpay__KeyId, ...), never appsettings.json. Each paid plan is an auto-renewing plan created
// in the Razorpay dashboard; its amount and period live there, not in code. The launch offers are
// dashboard Offers (e.g. 25% off the first cycle), optional: leave empty for no launch discount.
public sealed class RazorpaySettings
{
    public const string SectionName = "Razorpay";

    public string ApiBaseUrl { get; set; } = string.Empty;
    public string KeyId { get; set; } = string.Empty;
    public string KeySecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string BasicPlanId { get; set; } = string.Empty;
    public string PremiumPlanId { get; set; } = string.Empty;
    public string BasicLaunchOfferId { get; set; } = string.Empty;
    public string PremiumLaunchOfferId { get; set; } = string.Empty;

    // Billing cycles before the subscription completes (Razorpay requires a finite count).
    public int TotalBillingCycles { get; set; }

    // How long a hosted checkout link stays payable; an unpaid one is reused until then.
    public int CheckoutLinkHours { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ApiBaseUrl)
        && !string.IsNullOrWhiteSpace(KeyId)
        && !string.IsNullOrWhiteSpace(KeySecret)
        && !string.IsNullOrWhiteSpace(WebhookSecret)
        && !string.IsNullOrWhiteSpace(BasicPlanId)
        && !string.IsNullOrWhiteSpace(PremiumPlanId)
        && TotalBillingCycles > 0
        && CheckoutLinkHours > 0;

    public string PlanIdFor(SubscriptionPlan plan) => plan switch
    {
        SubscriptionPlan.Basic => BasicPlanId,
        SubscriptionPlan.Premium => PremiumPlanId,
        _ => throw new ArgumentOutOfRangeException(nameof(plan), plan, "Only paid plans have a gateway plan.")
    };

    // Null when no launch offer is configured for the plan.
    public string? LaunchOfferIdFor(SubscriptionPlan plan)
    {
        var offerId = plan switch
        {
            SubscriptionPlan.Basic => BasicLaunchOfferId,
            SubscriptionPlan.Premium => PremiumLaunchOfferId,
            _ => string.Empty
        };
        return string.IsNullOrWhiteSpace(offerId) ? null : offerId;
    }

    public SubscriptionPlan? PlanFor(string gatewayPlanId) =>
        !string.IsNullOrEmpty(gatewayPlanId) && gatewayPlanId == BasicPlanId ? SubscriptionPlan.Basic
        : !string.IsNullOrEmpty(gatewayPlanId) && gatewayPlanId == PremiumPlanId ? SubscriptionPlan.Premium
        : null;
}
