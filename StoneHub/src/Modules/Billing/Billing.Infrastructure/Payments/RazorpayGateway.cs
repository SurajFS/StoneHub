using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Billing.Application.Payments;
using Billing.Domain;
using Microsoft.Extensions.Options;

namespace Billing.Infrastructure.Payments;

// Razorpay Subscriptions over plain HttpClient (no SDK: three calls don't justify a dependency).
// The client's base address, Basic auth and timeout are set at registration.
// API: https://razorpay.com/docs/api/payments/subscriptions/
public sealed class RazorpayGateway(HttpClient httpClient, IOptions<RazorpaySettings> options) : IPaymentGateway
{
    // Nulls are omitted so an absent offer_id isn't sent as an explicit null.
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private RazorpaySettings Settings => options.Value;

    public bool IsAvailable => Settings.IsConfigured;

    public async Task<GatewayCheckout> CreateRecurringCheckoutAsync(
        Guid userId, SubscriptionPlan plan, bool withLaunchOffer, CancellationToken ct = default)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddHours(Settings.CheckoutLinkHours);
        var request = new CreateSubscriptionRequest(
            Settings.PlanIdFor(plan),
            Settings.TotalBillingCycles,
            CustomerNotify: true,
            ExpireBy: expiresAt.ToUnixTimeSeconds(),
            Notes: new Dictionary<string, string> { [RazorpayWebhook.UserIdNote] = userId.ToString() },
            OfferId: withLaunchOffer ? Settings.LaunchOfferIdFor(plan) : null);

        using var response = await httpClient.PostAsJsonAsync("subscriptions", request, Json, ct);
        EnsureSuccess(response, "create subscription");

        var created = await response.Content.ReadFromJsonAsync<CreatedSubscription>(Json, ct);
        if (created is null || string.IsNullOrEmpty(created.Id) || string.IsNullOrEmpty(created.ShortUrl))
            throw new InvalidOperationException("Razorpay create subscription returned no id or short_url.");

        return new GatewayCheckout(created.Id, created.ShortUrl, expiresAt);
    }

    public Task CancelRenewalAsync(string gatewaySubscriptionId, CancellationToken ct = default) =>
        CancelAsync(gatewaySubscriptionId, atCycleEnd: true, ct);

    public Task CancelImmediatelyAsync(string gatewaySubscriptionId, CancellationToken ct = default) =>
        CancelAsync(gatewaySubscriptionId, atCycleEnd: false, ct);

    public bool IsAuthenticWebhook(string payload, string signature) =>
        RazorpayWebhook.IsAuthentic(payload, signature, Settings.WebhookSecret);

    public PaymentEvent? ParseWebhook(string payload) => RazorpayWebhook.Parse(payload);

    public SubscriptionPlan? PlanFor(string gatewayPlanId) => Settings.PlanFor(gatewayPlanId);

    private async Task CancelAsync(string gatewaySubscriptionId, bool atCycleEnd, CancellationToken ct)
    {
        var path = $"subscriptions/{Uri.EscapeDataString(gatewaySubscriptionId)}/cancel";
        using var response = await httpClient.PostAsJsonAsync(path, new CancelSubscriptionRequest(atCycleEnd), Json, ct);
        EnsureSuccess(response, atCycleEnd ? "cancel subscription at cycle end" : "cancel subscription");
    }

    // Only the status goes into the exception: Razorpay error bodies can echo customer details.
    private static void EnsureSuccess(HttpResponseMessage response, string operation)
    {
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Razorpay {operation} failed with status {(int)response.StatusCode}.", null, response.StatusCode);
    }

    private sealed record CreateSubscriptionRequest(
        string PlanId, int TotalCount, bool CustomerNotify, long ExpireBy, IReadOnlyDictionary<string, string> Notes,
        string? OfferId);

    private sealed record CancelSubscriptionRequest(bool CancelAtCycleEnd);

    private sealed record CreatedSubscription(string Id, string ShortUrl);
}
