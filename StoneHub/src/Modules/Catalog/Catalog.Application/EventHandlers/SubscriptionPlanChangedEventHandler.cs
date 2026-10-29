using Billing.Domain;
using Catalog.Domain;
using MediatR;

namespace Catalog.Application.EventHandlers;

// Basic ↔ Premium: the ranking boost follows the new plan.
public sealed class SubscriptionPlanChangedEventHandler(ISellerPromotionStatusRepository promotions)
    : INotificationHandler<SubscriptionPlanChangedEvent>
{
    public Task Handle(SubscriptionPlanChangedEvent notification, CancellationToken ct) =>
        promotions.SetPremiumAsync(notification.UserId, isPremium: notification.Plan == SubscriptionPlan.Premium, ct);
}
