namespace StoneHub.Api.RateLimiting;

// Bound from the "RateLimiting" config section; the defaults apply when it's absent.
public sealed class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    public FixedWindowSettings PaymentWebhook { get; set; } = new();
}

public sealed class FixedWindowSettings
{
    // Razorpay sends a handful of events per subscription per period, plus retries; 60 a minute
    // from one IP is far above legitimate traffic. A rejected delivery is retried by the gateway.
    public int PermitLimit { get; set; } = 60;
    public int WindowSeconds { get; set; } = 60;
}
