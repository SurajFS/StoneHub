using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Billing.Application.Payments;

namespace Billing.Infrastructure.Payments;

// Razorpay webhook verification and parsing, kept pure (no I/O) so it's unit-testable.
// Docs: https://razorpay.com/docs/webhooks/validate-test/ and /docs/webhooks/subscriptions/.
public static class RazorpayWebhook
{
    // The notes key carrying our user id, so a webhook can be matched back to the account.
    public const string UserIdNote = "user_id";

    private static readonly IReadOnlyDictionary<string, PaymentEventKind> Kinds = new Dictionary<string, PaymentEventKind>
    {
        ["subscription.activated"] = PaymentEventKind.Charged,
        ["subscription.charged"] = PaymentEventKind.Charged,
        ["subscription.cancelled"] = PaymentEventKind.RenewalEnded,
        ["subscription.halted"] = PaymentEventKind.RenewalEnded,
        ["subscription.completed"] = PaymentEventKind.RenewalEnded,
    };

    // X-Razorpay-Signature = hex HMAC-SHA256 of the raw body, keyed with the webhook secret.
    public static bool IsAuthentic(string payload, string signature, string webhookSecret)
    {
        if (string.IsNullOrEmpty(webhookSecret) || string.IsNullOrEmpty(signature))
            return false;

        var expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(webhookSecret), Encoding.UTF8.GetBytes(payload));
        var expectedHex = Encoding.ASCII.GetBytes(Convert.ToHexStringLower(expected));
        var actualHex = Encoding.ASCII.GetBytes(signature.Trim().ToLowerInvariant());
        return CryptographicOperations.FixedTimeEquals(expectedHex, actualHex);
    }

    // Reads event -> payload.subscription.entity { id, plan_id, current_end, notes.user_id }. Notes are an
    // object when set but an empty array when not, hence JsonDocument rather than typed records.
    public static PaymentEvent? Parse(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            if (!TryGetString(root, "event", out var eventType))
                return null;

            var kind = Kinds.GetValueOrDefault(eventType, PaymentEventKind.Ignored);
            if (!TryGetObject(root, out var entity, "payload", "subscription", "entity"))
                return new PaymentEvent(eventType, kind, null, null, null);

            var subscriptionId = TryGetString(entity, "id", out var id) ? id : null;
            var planId = TryGetString(entity, "plan_id", out var plan) ? plan : null;
            return new PaymentEvent(eventType, kind, subscriptionId, ReadUserId(entity), ReadPeriodEnd(entity), planId);
        }
        catch (JsonException)
        {
            // Malformed body: the caller treats null as an unreadable payload (400).
            return null;
        }
    }

    private static Guid? ReadUserId(JsonElement entity) =>
        TryGetObject(entity, out var notes, "notes")
        && TryGetString(notes, UserIdNote, out var raw)
        && Guid.TryParse(raw, out var userId)
            ? userId
            : null;

    private static DateTimeOffset? ReadPeriodEnd(JsonElement entity) =>
        entity.TryGetProperty("current_end", out var end)
        && end.ValueKind == JsonValueKind.Number
        && end.TryGetInt64(out var seconds)
            ? DateTimeOffset.FromUnixTimeSeconds(seconds)
            : null;

    private static bool TryGetObject(JsonElement start, out JsonElement found, params string[] path)
    {
        found = start;
        foreach (var name in path)
        {
            if (found.ValueKind != JsonValueKind.Object || !found.TryGetProperty(name, out found))
                return false;
        }
        return found.ValueKind == JsonValueKind.Object;
    }

    private static bool TryGetString(JsonElement element, string name, out string value)
    {
        value = string.Empty;
        if (element.ValueKind != JsonValueKind.Object
            || !element.TryGetProperty(name, out var property)
            || property.ValueKind != JsonValueKind.String)
            return false;

        value = property.GetString() ?? string.Empty;
        return value.Length > 0;
    }
}
