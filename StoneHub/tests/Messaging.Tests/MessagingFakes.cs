using Billing.Application;
using Catalog.Application;
using Identity.Application;
using Messaging.Application;
using Messaging.Application.Dtos;
using Messaging.Domain;

namespace Messaging.Tests;

// In-memory stand-ins for the plan-gate tests: one store behind both repositories and the query
// service, plus scripted users, listings and plan entitlements.
internal sealed class FakeMessagingStore : IConversationRepository, IMessageRepository, IMessagingQueryService
{
    private readonly List<Conversation> _conversations = [];

    public List<Message> Messages { get; } = [];

    public int SaveCount { get; private set; }

    public Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_conversations.FirstOrDefault(c => c.Id == id));

    public Task<Conversation?> GetByParticipantsAsync(Guid userOne, Guid userTwo, CancellationToken ct = default) =>
        Task.FromResult(_conversations.FirstOrDefault(c => c.HasParticipant(userOne) && c.HasParticipant(userTwo)));

    public void Add(Conversation conversation) => _conversations.Add(conversation);

    public void Add(Message message) => Messages.Add(message);

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(Guid userId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ConversationDto>>(_conversations
            .Where(c => c.HasParticipant(userId))
            .Select(c => new ConversationDto(c.Id, Guid.NewGuid(), "Counterpart", null, c.ProductId, c.ProductTitle,
                c.LastMessagePreview, c.LastMessageAt, UnreadCount: 1))
            .ToList());

    public Task<IReadOnlyList<MessageDto>> GetMessagesAsync(
        Guid conversationId, DateTimeOffset? since, int limit, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<MessageDto>>(Messages
            .Where(m => m.ConversationId == conversationId)
            .Select(m => new MessageDto(m.Id, m.ConversationId, m.SenderId, m.Kind.ToString(), m.Body, m.MediaUrl, m.SentAt))
            .ToList());
}

internal sealed class FakeEntitlements(params Guid[] lockedUsers) : IPlanEntitlements
{
    public Task<bool> CanUseOwnerFeaturesAsync(Guid userId, CancellationToken ct = default) =>
        Task.FromResult(!lockedUsers.Contains(userId));
}

internal sealed class FakeDirectory : IUserDirectory, IProductLookup
{
    private readonly Dictionary<Guid, UserDirectoryEntry> _users = [];
    private readonly Dictionary<Guid, ProductOwnerInfo> _products = [];

    public Guid AddUser(string role)
    {
        var id = Guid.NewGuid();
        _users[id] = new UserDirectoryEntry(id, role, role + " name", null);
        return id;
    }

    public Guid AddListing(Guid ownerId)
    {
        var id = Guid.NewGuid();
        _products[id] = new ProductOwnerInfo(ownerId, "Seller", "Makrana white", IsActive: true);
        return id;
    }

    public Task<UserDirectoryEntry?> GetAsync(Guid userId, CancellationToken ct = default) =>
        Task.FromResult(_users.GetValueOrDefault(userId));

    public Task<ProductOwnerInfo?> GetOwnerInfoAsync(Guid productId, CancellationToken ct = default) =>
        Task.FromResult(_products.GetValueOrDefault(productId));
}
