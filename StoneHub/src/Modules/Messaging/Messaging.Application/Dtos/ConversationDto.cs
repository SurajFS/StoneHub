namespace Messaging.Application.Dtos;

// One row in a user's chat list: the other participant, last-message preview, and how many
// messages from them the caller hasn't read yet.
public sealed record ConversationDto(
    Guid Id,
    Guid CounterpartId,
    string CounterpartName,
    string? CounterpartAvatarUrl,
    Guid? ProductId,
    string? ProductTitle,
    string? LastMessagePreview,
    DateTimeOffset? LastMessageAt,
    int UnreadCount);
