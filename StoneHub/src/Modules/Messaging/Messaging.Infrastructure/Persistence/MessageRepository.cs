using Messaging.Domain;

namespace Messaging.Infrastructure.Persistence;

public sealed class MessageRepository(MessagingDbContext dbContext) : IMessageRepository
{
    public void Add(Message message) => dbContext.Messages.Add(message);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
