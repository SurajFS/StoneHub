using SharedKernel;

namespace Identity.Domain;

public sealed record SellerRegisteredEvent(Guid SellerProfileId, Guid UserId, string CompanyName) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
