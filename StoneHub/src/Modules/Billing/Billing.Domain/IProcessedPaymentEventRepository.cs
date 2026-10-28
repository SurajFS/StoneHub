namespace Billing.Domain;

// Shares the Billing unit of work: an added event is saved by ISubscriptionRepository.SaveChangesAsync
// together with the subscription change it caused.
public interface IProcessedPaymentEventRepository
{
    Task<bool> ExistsAsync(string eventId, CancellationToken ct = default);

    void Add(ProcessedPaymentEvent paymentEvent);
}
