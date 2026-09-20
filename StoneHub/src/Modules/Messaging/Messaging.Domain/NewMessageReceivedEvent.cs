using SharedKernel;

namespace Messaging.Domain;

// Published so the Notifications module can push-alert the recipient without Messaging
// knowing anything about push delivery.
public sealed record NewMessageReceivedEvent(
    Guid ConversationId, Guid RecipientId, Guid SenderId, string SenderName, string Preview) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
