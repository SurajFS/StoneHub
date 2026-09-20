namespace Billing.Application.Dtos;

// SubscriptionDto plus the user's display info, for the admin subscriptions list —
// resolved via IUserDirectory at read time rather than denormalized into Billing's own table.
public sealed record AdminSubscriptionRowDto(
    Guid UserId,
    string UserName,
    string UserRole,
    string Plan,
    string Status,
    DateTimeOffset StartDate,
    DateTimeOffset? ExpiryDate,
    string? PaymentNotes);
