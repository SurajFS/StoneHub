namespace Billing.Application.Dtos;

public sealed record SubscriptionDto(
    Guid Id,
    Guid UserId,
    // Free, Basic or Premium.
    string Plan,
    string Status,
    DateTimeOffset StartDate,
    DateTimeOffset? ExpiryDate,
    string? PaymentNotes,
    DateTimeOffset CreatedAt,
    // True while the gateway will charge again at ExpiryDate; false = the plan ends then.
    bool WillRenew,
    // Whether a gateway charge ever succeeded; first-time subscribers get the launch offer.
    bool HasEverPaid);
