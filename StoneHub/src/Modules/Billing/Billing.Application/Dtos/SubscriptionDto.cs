namespace Billing.Application.Dtos;

public sealed record SubscriptionDto(
    Guid Id,
    Guid UserId,
    string Plan,
    string Status,
    DateTimeOffset StartDate,
    DateTimeOffset? ExpiryDate,
    string? PaymentNotes,
    DateTimeOffset CreatedAt);
