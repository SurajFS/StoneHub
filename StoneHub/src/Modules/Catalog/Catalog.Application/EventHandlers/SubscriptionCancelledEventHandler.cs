using Billing.Domain;
using Catalog.Domain;
using MediatR;

namespace Catalog.Application.EventHandlers;

public sealed class SubscriptionCancelledEventHandler(ISellerPromotionStatusRepository promotions)
    : INotificationHandler<SubscriptionCancelledEvent>
{
    public Task Handle(SubscriptionCancelledEvent notification, CancellationToken ct) =>
        promotions.SetPremiumAsync(notification.UserId, isPremium: false, ct);
}
