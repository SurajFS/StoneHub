using Messaging.Domain;
using Xunit;

namespace Messaging.Tests;

// The "who may chat whom" authorization matrix. StartConversationCommandHandler gates on this,
// so these are the authoritative rules for opening a thread.
public sealed class ChatPolicyTests
{
    [Theory]
    [InlineData(ChatPolicy.Buyer, ChatPolicy.Seller)]
    [InlineData(ChatPolicy.Seller, ChatPolicy.Buyer)]
    [InlineData(ChatPolicy.Seller, ChatPolicy.Wholesaler)]
    [InlineData(ChatPolicy.Wholesaler, ChatPolicy.Seller)]
    public void CanConverse_AllowedPairs_ReturnsTrue(string roleOne, string roleTwo) =>
        Assert.True(ChatPolicy.CanConverse(roleOne, roleTwo));

    [Theory]
    [InlineData(ChatPolicy.Buyer, ChatPolicy.Wholesaler)] // seller bridges these two; they can't talk directly
    [InlineData(ChatPolicy.Wholesaler, ChatPolicy.Buyer)]
    [InlineData(ChatPolicy.Buyer, ChatPolicy.Buyer)]       // same-role never allowed
    [InlineData(ChatPolicy.Seller, ChatPolicy.Seller)]
    [InlineData(ChatPolicy.Wholesaler, ChatPolicy.Wholesaler)]
    public void CanConverse_DisallowedPairs_ReturnsFalse(string roleOne, string roleTwo) =>
        Assert.False(ChatPolicy.CanConverse(roleOne, roleTwo));

    [Theory]
    [InlineData("buyer", "SELLER")]
    [InlineData("Seller", "wholesaler")]
    public void CanConverse_IsCaseInsensitive(string roleOne, string roleTwo) =>
        Assert.True(ChatPolicy.CanConverse(roleOne, roleTwo));

    [Theory]
    [InlineData("Admin", ChatPolicy.Seller)]
    [InlineData(ChatPolicy.Seller, "")]
    [InlineData("", "")]
    [InlineData("Guest", "Guest")]
    public void CanConverse_UnknownOrEmptyRole_ReturnsFalse(string roleOne, string roleTwo) =>
        Assert.False(ChatPolicy.CanConverse(roleOne, roleTwo));
}
