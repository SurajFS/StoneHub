using Messaging.Domain;
using SharedKernel;
using Xunit;

namespace Messaging.Tests;

// Message.Create's content rules: text needs a body, image needs a media URL, and the two
// kinds don't leak each other's payload. SendMessageCommandHandler relies on these.
public sealed class MessageTests
{
    private static readonly Guid ConversationId = Guid.NewGuid();
    private static readonly Guid SenderId = Guid.NewGuid();

    private static Result<Message> CreateText(string? body) =>
        Message.Create(ConversationId, SenderId, MessageKind.Text, body, null);

    [Fact]
    public void Create_Text_TrimsBodyAndCarriesNoMedia()
    {
        var result = Message.Create(ConversationId, SenderId, MessageKind.Text, "  hello  ", "https://cdn/x.jpg");

        Assert.True(result.IsSuccess);
        Assert.Equal(MessageKind.Text, result.Value.Kind);
        Assert.Equal("hello", result.Value.Body);
        Assert.Null(result.Value.MediaUrl); // a text message drops any media URL
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_TextWithoutBody_Fails(string? body) =>
        Assert.True(CreateText(body).IsFailure);

    [Fact]
    public void Create_Image_TrimsMediaAndAllowsNullCaption()
    {
        var result = Message.Create(ConversationId, SenderId, MessageKind.Image, null, "  https://cdn/photo.jpg  ");

        Assert.True(result.IsSuccess);
        Assert.Equal(MessageKind.Image, result.Value.Kind);
        Assert.Null(result.Value.Body);
        Assert.Equal("https://cdn/photo.jpg", result.Value.MediaUrl);
    }

    [Fact]
    public void Create_ImageWithCaption_KeepsBothTrimmed()
    {
        var result = Message.Create(ConversationId, SenderId, MessageKind.Image, "  nice slab  ", "https://cdn/photo.jpg");

        Assert.True(result.IsSuccess);
        Assert.Equal("nice slab", result.Value.Body);
        Assert.Equal("https://cdn/photo.jpg", result.Value.MediaUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ImageWithoutMedia_Fails(string? mediaUrl) =>
        Assert.True(Message.Create(ConversationId, SenderId, MessageKind.Image, "caption", mediaUrl).IsFailure);

    [Fact]
    public void Create_SetsConversationAndSender()
    {
        var message = CreateText("hi").Value;

        Assert.Equal(ConversationId, message.ConversationId);
        Assert.Equal(SenderId, message.SenderId);
    }
}
