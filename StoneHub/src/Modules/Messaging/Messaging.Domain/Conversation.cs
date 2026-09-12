using SharedKernel;

namespace Messaging.Domain;

// A 1:1 chat thread between two users (buyer↔seller or seller↔wholesaler). Participants are stored
// in a canonical order (A = lower user id) so the pair is unique regardless of who started it.
// Each side's display name + avatar are denormalized at creation so the conversation-list read
// stays inside this module (kept fresh by profile events later — deferred).
public sealed class Conversation : AggregateRoot<Guid>
{
    private const int PreviewLength = 140;

    public Guid ParticipantAId { get; private set; }
    public string ParticipantAName { get; private set; } = string.Empty;
    public string? ParticipantAAvatarUrl { get; private set; }
    public Guid ParticipantBId { get; private set; }
    public string ParticipantBName { get; private set; } = string.Empty;
    public string? ParticipantBAvatarUrl { get; private set; }
    public Guid? ProductId { get; private set; }
    public string? ProductTitle { get; private set; }
    public string? LastMessagePreview { get; private set; }
    public DateTimeOffset? LastMessageAt { get; private set; }
    public DateTimeOffset? ParticipantALastReadAt { get; private set; }
    public DateTimeOffset? ParticipantBLastReadAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Conversation() { }

    private Conversation(
        Guid id,
        Guid participantAId, string participantAName, string? participantAAvatarUrl,
        Guid participantBId, string participantBName, string? participantBAvatarUrl,
        Guid? productId, string? productTitle) : base(id)
    {
        ParticipantAId = participantAId;
        ParticipantAName = participantAName;
        ParticipantAAvatarUrl = participantAAvatarUrl;
        ParticipantBId = participantBId;
        ParticipantBName = participantBName;
        ParticipantBAvatarUrl = participantBAvatarUrl;
        ProductId = productId;
        ProductTitle = string.IsNullOrWhiteSpace(productTitle) ? null : productTitle.Trim();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // Canonicalize so (A, B) is order-independent and uniquely identifies the pair.
    public static Conversation Start(
        Guid callerId, string callerName, string? callerAvatarUrl,
        Guid ownerId, string ownerName, string? ownerAvatarUrl,
        Guid? productId, string? productTitle) =>
        callerId.CompareTo(ownerId) <= 0
            ? new Conversation(Guid.NewGuid(), callerId, callerName, callerAvatarUrl, ownerId, ownerName, ownerAvatarUrl, productId, productTitle)
            : new Conversation(Guid.NewGuid(), ownerId, ownerName, ownerAvatarUrl, callerId, callerName, callerAvatarUrl, productId, productTitle);

    public bool HasParticipant(Guid userId) => ParticipantAId == userId || ParticipantBId == userId;

    public void RecordMessage(string preview, DateTimeOffset sentAt)
    {
        LastMessagePreview = preview.Length > PreviewLength ? preview[..PreviewLength] : preview;
        LastMessageAt = sentAt;
    }

    public void MarkRead(Guid userId, DateTimeOffset at)
    {
        if (userId == ParticipantAId) ParticipantALastReadAt = at;
        else if (userId == ParticipantBId) ParticipantBLastReadAt = at;
    }
}
