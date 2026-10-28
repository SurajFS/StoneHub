using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;

namespace StoneHub.Api.RateLimiting;

public static class RateLimitingExtensions
{
    // Shared bucket for requests whose client address is unknown (shouldn't happen over TCP).
    private const string UnknownClient = "unknown";

    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(RateLimitingSettings.SectionName).Get<RateLimitingSettings>()
            ?? new RateLimitingSettings();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(RateLimitPolicies.PaymentWebhook, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? UnknownClient,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.PaymentWebhook.PermitLimit,
                        Window = TimeSpan.FromSeconds(settings.PaymentWebhook.WindowSeconds),
                        QueueLimit = 0
                    }));
        });

        // Production sits behind Caddy on loopback, so without this every request's remote IP is
        // 127.0.0.1 and per-IP limits collapse into one bucket. The defaults trust only loopback
        // proxies, so a client can't spoof X-Forwarded-For from outside.
        services.Configure<ForwardedHeadersOptions>(options =>
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

        return services;
    }
}
