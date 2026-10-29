using System.Security.Cryptography;
using System.Text;
using Billing.Application.Payments;
using Billing.Infrastructure.Payments;
using Xunit;

namespace Billing.Tests;

// Razorpay's signature check and payload shape: a wrong signature would let anyone grant
// themselves Premium, and a misread payload would grant the wrong period or the wrong account.
public sealed class RazorpayWebhookTests
{
    private const string Secret = "whsec_test";
    private const string GatewayId = "sub_ABC123";
    private const string GatewayPlanId = "plan_BASIC1";
    private const long CurrentEnd = 1_798_761_600; // 2027-01-01T00:00:00Z

    [Fact]
    public void IsAuthentic_CorrectSignature_ReturnsTrue()
    {
        var payload = ChargedPayload("{}");

        Assert.True(RazorpayWebhook.IsAuthentic(payload, Sign(payload, Secret), Secret));
    }

    [Fact]
    public void IsAuthentic_UppercaseSignatureWithWhitespace_ReturnsTrue()
    {
        var payload = ChargedPayload("{}");

        Assert.True(RazorpayWebhook.IsAuthentic(payload, $" {Sign(payload, Secret).ToUpperInvariant()} ", Secret));
    }

    [Fact]
    public void IsAuthentic_TamperedPayload_ReturnsFalse()
    {
        var payload = ChargedPayload("{}");
        var signature = Sign(payload, Secret);

        Assert.False(RazorpayWebhook.IsAuthentic(payload.Replace(GatewayId, "sub_OTHER"), signature, Secret));
    }

    [Fact]
    public void IsAuthentic_SignedWithDifferentSecret_ReturnsFalse()
    {
        var payload = ChargedPayload("{}");

        Assert.False(RazorpayWebhook.IsAuthentic(payload, Sign(payload, "whsec_other"), Secret));
    }

    [Theory]
    [InlineData("", "abc")]
    [InlineData(Secret, "")]
    public void IsAuthentic_MissingSecretOrSignature_ReturnsFalse(string secret, string signature)
    {
        Assert.False(RazorpayWebhook.IsAuthentic(ChargedPayload("{}"), signature, secret));
    }

    [Theory]
    [InlineData("subscription.activated", PaymentEventKind.Charged)]
    [InlineData("subscription.charged", PaymentEventKind.Charged)]
    [InlineData("subscription.cancelled", PaymentEventKind.RenewalEnded)]
    [InlineData("subscription.halted", PaymentEventKind.RenewalEnded)]
    [InlineData("subscription.completed", PaymentEventKind.RenewalEnded)]
    [InlineData("subscription.pending", PaymentEventKind.Ignored)]
    [InlineData("payment.captured", PaymentEventKind.Ignored)]
    public void Parse_MapsEventTypeToKind(string eventType, PaymentEventKind expected)
    {
        var parsed = RazorpayWebhook.Parse(Payload(eventType, "{}", CurrentEnd.ToString()));

        Assert.NotNull(parsed);
        Assert.Equal(eventType, parsed.EventType);
        Assert.Equal(expected, parsed.Kind);
    }

    [Fact]
    public void Parse_Charged_ReadsSubscriptionIdPlanIdUserIdAndPeriodEnd()
    {
        var userId = Guid.NewGuid();

        var parsed = RazorpayWebhook.Parse(ChargedPayload($$"""{"{{RazorpayWebhook.UserIdNote}}":"{{userId}}"}"""));

        Assert.NotNull(parsed);
        Assert.Equal(GatewayId, parsed.GatewaySubscriptionId);
        Assert.Equal(GatewayPlanId, parsed.GatewayPlanId);
        Assert.Equal(userId, parsed.UserId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(CurrentEnd), parsed.PeriodEnd);
    }

    // Razorpay sends notes as [] when none were set.
    [Fact]
    public void Parse_NotesAsEmptyArray_HasNoUserId()
    {
        var parsed = RazorpayWebhook.Parse(ChargedPayload("[]"));

        Assert.NotNull(parsed);
        Assert.Null(parsed.UserId);
        Assert.Equal(GatewayId, parsed.GatewaySubscriptionId);
    }

    [Fact]
    public void Parse_UserIdNoteNotAGuid_HasNoUserId()
    {
        var parsed = RazorpayWebhook.Parse(ChargedPayload($$"""{"{{RazorpayWebhook.UserIdNote}}":"not-a-guid"}"""));

        Assert.NotNull(parsed);
        Assert.Null(parsed.UserId);
    }

    [Fact]
    public void Parse_NullCurrentEnd_HasNoPeriodEnd()
    {
        var parsed = RazorpayWebhook.Parse(Payload("subscription.activated", "{}", "null"));

        Assert.NotNull(parsed);
        Assert.Null(parsed.PeriodEnd);
    }

    [Fact]
    public void Parse_NoSubscriptionEntity_KeepsEventTypeWithoutIds()
    {
        var parsed = RazorpayWebhook.Parse("""{"event":"subscription.charged","payload":{}}""");

        Assert.NotNull(parsed);
        Assert.Equal(PaymentEventKind.Charged, parsed.Kind);
        Assert.Null(parsed.GatewaySubscriptionId);
        Assert.Null(parsed.PeriodEnd);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("""{"payload":{}}""")]
    [InlineData("""{"event":42}""")]
    [InlineData("[]")]
    public void Parse_MalformedOrMissingEvent_ReturnsNull(string payload)
    {
        Assert.Null(RazorpayWebhook.Parse(payload));
    }

    private static string ChargedPayload(string notesJson) =>
        Payload("subscription.charged", notesJson, CurrentEnd.ToString());

    private static string Payload(string eventType, string notesJson, string currentEndJson) =>
        $$$$"""
        {"entity":"event","event":"{{{{eventType}}}}","payload":{"subscription":{"entity":
        {"id":"{{{{GatewayId}}}}","plan_id":"{{{{GatewayPlanId}}}}","status":"active","current_end":{{{{currentEndJson}}}},"notes":{{{{notesJson}}}} }} }}
        """;

    private static string Sign(string payload, string secret) =>
        Convert.ToHexStringLower(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload)));
}
