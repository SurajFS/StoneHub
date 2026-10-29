using Billing.Domain;
using Catalog.Domain;
using MediatR;

namespace Catalog.Application.EventHandlers;

// Only Premium earns the ranking boost; Basic is a paid plan without it.
public sealed class SubscriptionActivatedEventHandler(ISellerPromotionStatusRepository promotions)
    : INotificationHandler<SubscriptionActivatedEvent>
{
    public Task Handle(SubscriptionActivatedEvent notification, CancellationToken ct) =>
        promotions.SetPremiumAsync(notification.UserId, isPremium: notification.Plan == SubscriptionPlan.Premium, ct);
}
