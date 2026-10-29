namespace Messaging.Application.Dtos;

// One row in a user's chat list: the other participant, last-message preview, and how many
// messages from them the caller hasn't read yet. IsLocked: the caller is a Free seller/wholesaler, so
// the preview is withheld until they subscribe.
public sealed record ConversationDto(
    Guid Id,
    Guid CounterpartId,
    string CounterpartName,
    string? CounterpartAvatarUrl,
    Guid? ProductId,
    string? ProductTitle,
    string? LastMessagePreview,
    DateTimeOffset? LastMessageAt,
    int UnreadCount,
    bool IsLocked = false);
