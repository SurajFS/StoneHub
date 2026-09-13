namespace Messaging.Domain;

public interface IMessageRepository
{
    void Add(Message message);
    Task SaveChangesAsync(CancellationToken ct = default);
}
