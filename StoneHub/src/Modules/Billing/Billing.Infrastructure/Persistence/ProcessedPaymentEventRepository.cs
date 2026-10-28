using Billing.Domain;
using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure.Persistence;

public sealed class ProcessedPaymentEventRepository(BillingDbContext dbContext) : IProcessedPaymentEventRepository
{
    public Task<bool> ExistsAsync(string eventId, CancellationToken ct = default) =>
        dbContext.ProcessedPaymentEvents.AnyAsync(x => x.Id == eventId, ct);

    public void Add(ProcessedPaymentEvent paymentEvent) => dbContext.ProcessedPaymentEvents.Add(paymentEvent);
}
