using SharedKernel;

namespace Billing.Domain;

public sealed record SubscriptionExpiredEvent(Guid UserId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
