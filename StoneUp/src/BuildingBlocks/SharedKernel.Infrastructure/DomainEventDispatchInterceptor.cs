using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SharedKernel.Infrastructure;

// Publishes each aggregate's domain events in-process (via MediatR) after the transaction
// commits, then clears them. This is at-most-once dispatch — acceptable for the modular-
// monolith MVP; a transactional outbox is the planned hardening if a reaction must never
// be lost on a crash between commit and publish.
//
// Only the async path is intercepted because every repository persists via SaveChangesAsync;
// dispatching synchronously would require sync-over-async, which the stack rules forbid.
public sealed class DomainEventDispatchInterceptor(IPublisher publisher) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            await DispatchEventsAsync(eventData.Context, cancellationToken);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchEventsAsync(DbContext context, CancellationToken ct)
    {
        var aggregates = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        var domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();

        // Clear first so a handler that re-saves the same context can't re-dispatch the
        // events currently being processed.
        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
            await publisher.Publish(domainEvent, ct);
    }
}
