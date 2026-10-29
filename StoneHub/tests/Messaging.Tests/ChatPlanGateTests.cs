using Messaging.Application;
using Messaging.Domain;
using SharedKernel;
using Xunit;

namespace Messaging.Tests;

// Chat on a Free seller/wholesaler account: anyone may message them, but they can't start chats,
// read message text, or reply until they subscribe (402). Paying owners and buyers are untouched.
public sealed class ChatPlanGateTests
{
    private readonly FakeMessagingStore _store = new();
    private readonly FakeDirectory _directory = new();

    [Fact]
    public async Task BuyerCanStartChat_WithFreeSeller()
    {
        var (buyer, seller, listing) = BuyerAndSeller();

        var result = await new StartConversationCommandHandler(_store, _directory, _directory, new FakeEntitlements(seller))
            .Handle(new StartConversationCommand(buyer, listing), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task FreeOwner_CannotStartChat()
    {
        var seller = _directory.AddUser("Seller");
        var wholesaler = _directory.AddUser("Wholesaler");
        var listing = _directory.AddListing(wholesaler);

        var result = await new StartConversationCommandHandler(_store, _directory, _directory, new FakeEntitlements(seller))
            .Handle(new StartConversationCommand(seller, listing), CancellationToken.None);

        Assert.Equal(ErrorType.PaymentRequired, result.ErrorType);
        Assert.Equal(0, _store.SaveCount);
    }

    [Fact]
    public async Task BuyerCanSend_ToFreeSeller()
    {
        var (buyer, seller, conversation) = Conversation();

        var result = await Send(conversation, buyer, new FakeEntitlements(seller));

        Assert.True(result.IsSuccess);
        Assert.Single(_store.Messages);
    }

    [Fact]
    public async Task FreeSeller_CannotReply_NothingSaved()
    {
        var (_, seller, conversation) = Conversation();

        var result = await Send(conversation, seller, new FakeEntitlements(seller));

        Assert.Equal(ErrorType.PaymentRequired, result.ErrorType);
        Assert.Empty(_store.Messages);
        Assert.Equal(0, _store.SaveCount);
    }

    [Fact]
    public async Task FreeSeller_CannotReadMessages_PayingSellerCan()
    {
        var (buyer, seller, conversation) = Conversation();
        await Send(conversation, buyer, new FakeEntitlements());

        var locked = await new GetMessagesQueryHandler(_store, _store, new FakeEntitlements(seller))
            .Handle(new GetMessagesQuery(conversation, seller, null), CancellationToken.None);
        var paid = await new GetMessagesQueryHandler(_store, _store, new FakeEntitlements())
            .Handle(new GetMessagesQuery(conversation, seller, null), CancellationToken.None);

        Assert.Equal(ErrorType.PaymentRequired, locked.ErrorType);
        Assert.Equal("Hello, is this available?", Assert.Single(paid.Value).Body);
    }

    [Fact]
    public async Task FreeSellerChatList_ShowsConversationButWithholdsPreview()
    {
        var (buyer, seller, conversation) = Conversation();
        await Send(conversation, buyer, new FakeEntitlements());

        var locked = await new GetConversationsQueryHandler(_store, new FakeEntitlements(seller))
            .Handle(new GetConversationsQuery(seller), CancellationToken.None);
        var buyerView = await new GetConversationsQueryHandler(_store, new FakeEntitlements(seller))
            .Handle(new GetConversationsQuery(buyer), CancellationToken.None);

        var row = Assert.Single(locked);
        Assert.True(row.IsLocked);
        Assert.Null(row.LastMessagePreview);
        Assert.Equal(1, row.UnreadCount);
        Assert.False(Assert.Single(buyerView).IsLocked);
        Assert.Equal("Hello, is this available?", buyerView[0].LastMessagePreview);
    }

    private Task<Result<Application.Dtos.MessageDto>> Send(Guid conversationId, Guid senderId, FakeEntitlements entitlements) =>
        new SendMessageCommandHandler(_store, _store, entitlements)
            .Handle(new SendMessageCommand(conversationId, senderId, MessageKind.Text, "Hello, is this available?", null), CancellationToken.None);

    private (Guid Buyer, Guid Seller, Guid Listing) BuyerAndSeller()
    {
        var buyer = _directory.AddUser("Buyer");
        var seller = _directory.AddUser("Seller");
        return (buyer, seller, _directory.AddListing(seller));
    }

    private (Guid Buyer, Guid Seller, Guid ConversationId) Conversation()
    {
        var (buyer, seller, listing) = BuyerAndSeller();
        var conversation = Domain.Conversation.Start(buyer, "Buyer name", null, seller, "Seller name", null, listing, "Makrana white");
        _store.Add(conversation);
        return (buyer, seller, conversation.Id);
    }
}
