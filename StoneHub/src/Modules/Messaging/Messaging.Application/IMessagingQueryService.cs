using Messaging.Application.Dtos;

namespace Messaging.Application;

public interface IMessagingQueryService
{
    Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(Guid userId, CancellationToken ct = default);

    // `since` null → the most recent page (oldest→newest). `since` set → only messages after it,
    // for polling. Both return ascending so the client appends.
    Task<IReadOnlyList<MessageDto>> GetMessagesAsync(
        Guid conversationId, DateTimeOffset? since, int limit, CancellationToken ct = default);
}
