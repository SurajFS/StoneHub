using Advertising.Domain;
using Billing.Domain;
using MediatR;

namespace Advertising.Application.EventHandlers;

// Sponsored campaigns are a Premium benefit, so moving to Basic stops them, same as an expiry.
public sealed class SubscriptionPlanChangedEventHandler(ICampaignRepository campaigns)
    : INotificationHandler<SubscriptionPlanChangedEvent>
{
    public async Task Handle(SubscriptionPlanChangedEvent notification, CancellationToken ct)
    {
        if (notification.Plan == SubscriptionPlan.Premium)
            return;

        var active = await campaigns.GetActiveBySellerIdAsync(notification.UserId, ct);
        if (active.Count == 0)
            return;

        foreach (var campaign in active)
            campaign.Stop();

        await campaigns.SaveChangesAsync(ct);
    }
}
