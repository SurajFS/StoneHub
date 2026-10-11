using SharedKernel;

namespace Identity.Domain;

// A Seller's or Wholesaler's display name or location changed. Catalog listens so the owner's
// name and location copied onto their listings (search, cards) don't go stale.
public sealed record OwnerProfileChangedEvent(Guid UserId, string Name, string Location) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
