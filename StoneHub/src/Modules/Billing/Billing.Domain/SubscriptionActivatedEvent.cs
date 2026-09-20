using SharedKernel;

namespace Billing.Domain;

// Published so other modules (Catalog's ranking/search, once built) can react without
// reaching into Billing's tables.
public sealed record SubscriptionActivatedEvent(Guid UserId, DateTimeOffset ExpiryDate) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
