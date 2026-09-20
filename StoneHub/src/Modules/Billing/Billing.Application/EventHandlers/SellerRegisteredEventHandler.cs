using Billing.Domain;
using Identity.Domain;
using MediatR;

namespace Billing.Application.EventHandlers;

// Every Seller starts on the Free plan. Reacts to Identity's published domain event rather
// than Billing reaching into Identity's tables.
public sealed class SellerRegisteredEventHandler(ISubscriptionRepository subscriptions)
    : INotificationHandler<SellerRegisteredEvent>
{
    public async Task Handle(SellerRegisteredEvent notification, CancellationToken ct)
    {
        var existing = await subscriptions.GetByUserIdAsync(notification.UserId, ct);
        if (existing is not null)
            return;

        subscriptions.Add(Subscription.CreateFree(notification.UserId));
        await subscriptions.SaveChangesAsync(ct);
    }
}
