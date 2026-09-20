using Billing.Domain;
using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure.Persistence;

public sealed class SubscriptionRepository(BillingDbContext dbContext) : ISubscriptionRepository
{
    public Task<Subscription?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        dbContext.Subscriptions.FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public async Task<IReadOnlyList<Subscription>> GetDueForExpiryAsync(DateTimeOffset now, CancellationToken ct = default) =>
        await dbContext.Subscriptions
            .Where(x => x.Plan == SubscriptionPlan.Premium && x.Status == SubscriptionStatus.Active && x.ExpiryDate != null && x.ExpiryDate <= now)
            .ToListAsync(ct);

    public void Add(Subscription subscription) => dbContext.Subscriptions.Add(subscription);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
