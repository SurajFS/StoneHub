using Billing.Domain;
using Catalog.Domain;
using MediatR;

namespace Catalog.Application.EventHandlers;

public sealed class SubscriptionActivatedEventHandler(ISellerPromotionStatusRepository promotions)
    : INotificationHandler<SubscriptionActivatedEvent>
{
    public Task Handle(SubscriptionActivatedEvent notification, CancellationToken ct) =>
        promotions.SetPremiumAsync(notification.UserId, isPremium: true, ct);
}
