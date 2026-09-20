using Billing.Domain;
using Catalog.Domain;
using MediatR;

namespace Catalog.Application.EventHandlers;

public sealed class SubscriptionExpiredEventHandler(ISellerPromotionStatusRepository promotions)
    : INotificationHandler<SubscriptionExpiredEvent>
{
    public Task Handle(SubscriptionExpiredEvent notification, CancellationToken ct) =>
        promotions.SetPremiumAsync(notification.UserId, isPremium: false, ct);
}
