namespace StoneHub.Api.RateLimiting;

// Names of the rate-limit policies, referenced by [EnableRateLimiting] on the endpoints they guard.
public static class RateLimitPolicies
{
    // Anonymous, signature-checked payment-gateway webhooks. Each delivery costs an HMAC and a
    // DB read before it can be rejected, so floods are capped per client IP.
    public const string PaymentWebhook = "payment-webhook";
}
