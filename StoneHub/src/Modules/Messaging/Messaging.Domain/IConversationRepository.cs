namespace Messaging.Domain;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    // Finds an existing thread for a pair of users, order-independent.
    Task<Conversation?> GetByParticipantsAsync(Guid userOne, Guid userTwo, CancellationToken ct = default);
    void Add(Conversation conversation);
    Task SaveChangesAsync(CancellationToken ct = default);
}
