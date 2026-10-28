namespace Billing.Application.Dtos;

public sealed record SubscriptionDto(
    Guid Id,
    Guid UserId,
    string Plan,
    string Status,
    DateTimeOffset StartDate,
    DateTimeOffset? ExpiryDate,
    string? PaymentNotes,
    DateTimeOffset CreatedAt,
    // True while the gateway will charge again at ExpiryDate; false = Premium ends then.
    bool WillRenew);
