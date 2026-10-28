namespace Billing.Infrastructure.Payments;

// Razorpay config. Keys and the webhook secret come from user-secrets / the environment
// (Razorpay__KeyId, ...), never appsettings.json. PlanId is the auto-renewing Premium plan
// created in the Razorpay dashboard; its amount and period live there, not in code.
public sealed class RazorpaySettings
{
    public const string SectionName = "Razorpay";

    public string ApiBaseUrl { get; set; } = string.Empty;
    public string KeyId { get; set; } = string.Empty;
    public string KeySecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;

    // Billing cycles before the subscription completes (Razorpay requires a finite count).
    public int TotalBillingCycles { get; set; }

    // How long a hosted checkout link stays payable; an unpaid one is reused until then.
    public int CheckoutLinkHours { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ApiBaseUrl)
        && !string.IsNullOrWhiteSpace(KeyId)
        && !string.IsNullOrWhiteSpace(KeySecret)
        && !string.IsNullOrWhiteSpace(WebhookSecret)
        && !string.IsNullOrWhiteSpace(PlanId)
        && TotalBillingCycles > 0
        && CheckoutLinkHours > 0;
}
