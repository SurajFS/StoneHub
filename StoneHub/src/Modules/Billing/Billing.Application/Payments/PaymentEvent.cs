namespace Billing.Application.Payments;

// A gateway webhook reduced to what Billing needs. UserId comes from the notes attached at
// checkout, so a charge can still be matched if the stored gateway id was since replaced.
public sealed record PaymentEvent(
    string EventType,
    PaymentEventKind Kind,
    string? GatewaySubscriptionId,
    Guid? UserId,
    DateTimeOffset? PeriodEnd);
