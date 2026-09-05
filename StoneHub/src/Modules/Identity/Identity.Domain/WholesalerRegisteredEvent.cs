using SharedKernel;

namespace Identity.Domain;

public sealed record WholesalerRegisteredEvent(Guid WholesalerProfileId, Guid UserId, string BusinessName) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
