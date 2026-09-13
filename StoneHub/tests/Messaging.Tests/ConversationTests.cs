using Messaging.Domain;
using Xunit;

namespace Messaging.Tests;

// Conversation aggregate behaviour: canonical participant ordering (one thread per pair,
// order-independent), the HasParticipant guard the send/read handlers authorize on, the list
// preview, and per-side read tracking.
public sealed class ConversationTests
{
    private static readonly Guid ProductId = Guid.NewGuid();

    // Two ids where `low` sorts before `high` by Guid.CompareTo — the order Conversation canonicalizes on.
    private static (Guid low, Guid high) OrderedPair()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        return a.CompareTo(b) <= 0 ? (a, b) : (b, a);
    }

    [Fact]
    public void Start_CanonicalizesParticipants_RegardlessOfCallerOrder()
    {
        var (low, high) = OrderedPair();

        // The person with the lower id is "Alice", higher is "Bob" — no matter who opens the thread.
        var callerLow = Conversation.Start(low, "Alice", "aliceAvatar", high, "Bob", "bobAvatar", ProductId, "Slab");
        var callerHigh = Conversation.Start(high, "Bob", "bobAvatar", low, "Alice", "aliceAvatar", ProductId, "Slab");

        foreach (var convo in new[] { callerLow, callerHigh })
        {
            Assert.Equal(low, convo.ParticipantAId);
            Assert.Equal("Alice", convo.ParticipantAName);
            Assert.Equal("aliceAvatar", convo.ParticipantAAvatarUrl);
            Assert.Equal(high, convo.ParticipantBId);
            Assert.Equal("Bob", convo.ParticipantBName);
            Assert.Equal("bobAvatar", convo.ParticipantBAvatarUrl);
        }
    }

    [Fact]
    public void Start_BlankProductTitle_NormalizesToNull()
    {
        var (low, high) = OrderedPair();

        var convo = Conversation.Start(low, "Alice", null, high, "Bob", null, ProductId, "   ");

        Assert.Null(convo.ProductTitle);
    }

    [Fact]
    public void Start_ProductTitle_IsTrimmed()
    {
        var (low, high) = OrderedPair();

        var convo = Conversation.Start(low, "Alice", null, high, "Bob", null, ProductId, "  Slab  ");

        Assert.Equal("Slab", convo.ProductTitle);
    }

    [Fact]
    public void HasParticipant_TrueForBothMembers_FalseForStranger()
    {
        var (low, high) = OrderedPair();
        var convo = Conversation.Start(low, "Alice", null, high, "Bob", null, null, null);

        Assert.True(convo.HasParticipant(low));
        Assert.True(convo.HasParticipant(high));
        Assert.False(convo.HasParticipant(Guid.NewGuid()));
    }

    [Fact]
    public void RecordMessage_SetsPreviewAndTimestamp()
    {
        var convo = NewConversation();
        var at = DateTimeOffset.UtcNow;

        convo.RecordMessage("hello", at);

        Assert.Equal("hello", convo.LastMessagePreview);
        Assert.Equal(at, convo.LastMessageAt);
    }

    [Fact]
    public void RecordMessage_LongBody_TruncatesPreviewTo140()
    {
        var convo = NewConversation();

        convo.RecordMessage(new string('x', 200), DateTimeOffset.UtcNow);

        Assert.Equal(140, convo.LastMessagePreview!.Length);
    }

    [Fact]
    public void MarkRead_SetsOnlyTheCallingSide()
    {
        var (low, high) = OrderedPair();
        var convo = Conversation.Start(low, "Alice", null, high, "Bob", null, null, null);
        var at = DateTimeOffset.UtcNow;

        convo.MarkRead(convo.ParticipantAId, at);

        Assert.Equal(at, convo.ParticipantALastReadAt);
        Assert.Null(convo.ParticipantBLastReadAt);
    }

    [Fact]
    public void MarkRead_NonParticipant_IsNoOp()
    {
        var convo = NewConversation();

        convo.MarkRead(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Null(convo.ParticipantALastReadAt);
        Assert.Null(convo.ParticipantBLastReadAt);
    }

    private static Conversation NewConversation()
    {
        var (low, high) = OrderedPair();
        return Conversation.Start(low, "Alice", null, high, "Bob", null, null, null);
    }
}
