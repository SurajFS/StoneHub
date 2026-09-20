namespace Billing.Domain;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    // For the daily expiry sweep: Premium + Active rows past their expiry date.
    Task<IReadOnlyList<Subscription>> GetDueForExpiryAsync(DateTimeOffset now, CancellationToken ct = default);

    void Add(Subscription subscription);

    Task SaveChangesAsync(CancellationToken ct = default);
}
