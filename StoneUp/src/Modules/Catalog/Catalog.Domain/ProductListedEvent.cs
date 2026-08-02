using SharedKernel;

namespace Catalog.Domain;

public sealed record ProductListedEvent(Guid ProductId, Guid SellerId, string Title) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
