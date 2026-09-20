using Billing.Domain;
using Identity.Domain;
using MediatR;

namespace Billing.Application.EventHandlers;

public sealed class WholesalerRegisteredEventHandler(ISubscriptionRepository subscriptions)
    : INotificationHandler<WholesalerRegisteredEvent>
{
    public async Task Handle(WholesalerRegisteredEvent notification, CancellationToken ct)
    {
        var existing = await subscriptions.GetByUserIdAsync(notification.UserId, ct);
        if (existing is not null)
            return;

        subscriptions.Add(Subscription.CreateFree(notification.UserId));
        await subscriptions.SaveChangesAsync(ct);
    }
}
