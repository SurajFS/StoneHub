using SharedKernel;

namespace Billing.Domain;

public sealed record SubscriptionCancelledEvent(Guid UserId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
