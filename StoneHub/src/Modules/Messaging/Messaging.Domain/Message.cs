using SharedKernel;

namespace Messaging.Domain;

// A single chat message within a conversation. Its own aggregate so a thread is paged and polled
// without loading it through the Conversation. Image messages carry an R2 URL (uploaded via the
// Media pipeline, same as avatars/listing photos); the body is then an optional caption.
public sealed class Message : AggregateRoot<Guid>
{
    public Guid ConversationId { get; private set; }
    public Guid SenderId { get; private set; }
    public MessageKind Kind { get; private set; }
    public string? Body { get; private set; }
    public string? MediaUrl { get; private set; }
    public DateTimeOffset SentAt { get; private set; }

    private Message() { }

    private Message(
        Guid id, Guid conversationId, Guid senderId, MessageKind kind, string? body, string? mediaUrl) : base(id)
    {
        ConversationId = conversationId;
        SenderId = senderId;
        Kind = kind;
        Body = body;
        MediaUrl = mediaUrl;
        SentAt = DateTimeOffset.UtcNow;
    }

    public static Result<Message> Create(
        Guid conversationId, Guid senderId, MessageKind kind, string? body, string? mediaUrl)
    {
        var trimmedBody = string.IsNullOrWhiteSpace(body) ? null : body.Trim();
        var trimmedMedia = string.IsNullOrWhiteSpace(mediaUrl) ? null : mediaUrl.Trim();

        if (kind == MessageKind.Image)
        {
            if (trimmedMedia is null)
                return Result.Failure<Message>("An image message needs a media URL.");
        }
        else
        {
            if (trimmedBody is null)
                return Result.Failure<Message>("Message cannot be empty.");
            trimmedMedia = null; // a text message carries no media
        }

        return Result.Success(new Message(Guid.NewGuid(), conversationId, senderId, kind, trimmedBody, trimmedMedia));
    }
}
