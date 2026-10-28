using SharedKernel;

namespace Billing.Domain;

// A payment-gateway webhook event that has been applied, keyed by the gateway's event id.
// Gateways deliver at least once and retry on any non-2xx, so each event is recorded in the
// same save as its effect and a repeat delivery is acknowledged without being applied twice.
public sealed class ProcessedPaymentEvent : Entity<string>
{
    public string EventType { get; private set; } = string.Empty;
    public DateTimeOffset ProcessedAt { get; private set; }

    private ProcessedPaymentEvent() { }

    private ProcessedPaymentEvent(string eventId, string eventType) : base(eventId)
    {
        EventType = eventType;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public static ProcessedPaymentEvent Record(string eventId, string eventType) => new(eventId, eventType);
}
