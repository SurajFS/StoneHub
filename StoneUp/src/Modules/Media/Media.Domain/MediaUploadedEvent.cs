using SharedKernel;

namespace Media.Domain;

public sealed record MediaUploadedEvent(Guid MediaId, Guid OwnerId, string Key, string Url) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
