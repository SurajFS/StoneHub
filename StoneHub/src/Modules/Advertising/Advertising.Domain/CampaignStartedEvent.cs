using SharedKernel;

namespace Advertising.Domain;

// Published so Catalog can mark the product Sponsored without reaching into Advertising's tables.
public sealed record CampaignStartedEvent(Guid ProductId, Guid SellerId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
