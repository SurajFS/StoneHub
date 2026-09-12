using Messaging.Domain;
using Microsoft.EntityFrameworkCore;

namespace Messaging.Infrastructure.Persistence;

public sealed class ConversationRepository(MessagingDbContext dbContext) : IConversationRepository
{
    public Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.Conversations.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Conversation?> GetByParticipantsAsync(Guid userOne, Guid userTwo, CancellationToken ct = default)
    {
        // Participants are stored canonically (A = lower id); order the lookup the same way.
        var (a, b) = userOne.CompareTo(userTwo) <= 0 ? (userOne, userTwo) : (userTwo, userOne);
        return dbContext.Conversations.FirstOrDefaultAsync(x => x.ParticipantAId == a && x.ParticipantBId == b, ct);
    }

    public void Add(Conversation conversation) => dbContext.Conversations.Add(conversation);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
