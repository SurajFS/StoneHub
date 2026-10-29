using SharedKernel;

namespace Billing.Domain;

// A Free account moved onto a paid plan (Basic or Premium). Published so other modules (Catalog's
// ranking, Advertising) can react without reaching into Billing's tables.
public sealed record SubscriptionActivatedEvent(Guid UserId, DateTimeOffset ExpiryDate, SubscriptionPlan Plan) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
