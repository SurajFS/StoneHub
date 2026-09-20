using SharedKernel;

namespace Advertising.Domain;

public sealed record CampaignStoppedEvent(Guid ProductId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
