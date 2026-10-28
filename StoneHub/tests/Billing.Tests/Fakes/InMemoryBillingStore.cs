using Billing.Domain;

namespace Billing.Tests.Fakes;

// One fake unit of work behind both Billing repositories, mirroring BillingDbContext: added
// payment events only become visible once SaveChangesAsync runs, so tests can assert that a
// rejected webhook saved nothing.
public sealed class InMemoryBillingStore : ISubscriptionRepository, IProcessedPaymentEventRepository
{
    private readonly List<Subscription> _subscriptions = [];
    private readonly List<ProcessedPaymentEvent> _pendingEvents = [];
    private readonly List<ProcessedPaymentEvent> _savedEvents = [];

    public int SaveCount { get; private set; }

    public IReadOnlyList<ProcessedPaymentEvent> SavedEvents => _savedEvents;

    public Task<Subscription?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        Task.FromResult(_subscriptions.FirstOrDefault(x => x.UserId == userId));

    public Task<Subscription?> GetByGatewaySubscriptionIdAsync(string gatewaySubscriptionId, CancellationToken ct = default) =>
        Task.FromResult(_subscriptions.FirstOrDefault(x => x.GatewaySubscriptionId == gatewaySubscriptionId));

    public Task<IReadOnlyList<Subscription>> GetDueForExpiryAsync(DateTimeOffset now, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Subscription>>(
            _subscriptions.Where(x => x.IsPremiumActive && x.ExpiryDate <= now).ToList());

    public void Add(Subscription subscription) => _subscriptions.Add(subscription);

    public Task<bool> ExistsAsync(string eventId, CancellationToken ct = default) =>
        Task.FromResult(_savedEvents.Any(x => x.Id == eventId));

    public void Add(ProcessedPaymentEvent paymentEvent) => _pendingEvents.Add(paymentEvent);

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        _savedEvents.AddRange(_pendingEvents);
        _pendingEvents.Clear();
        SaveCount++;
        return Task.CompletedTask;
    }
}
