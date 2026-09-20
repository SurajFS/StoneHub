using Advertising.Domain;
using Billing.Domain;
using MediatR;

namespace Advertising.Application.EventHandlers;

public sealed class SubscriptionCancelledEventHandler(ICampaignRepository campaigns)
    : INotificationHandler<SubscriptionCancelledEvent>
{
    public async Task Handle(SubscriptionCancelledEvent notification, CancellationToken ct)
    {
        var active = await campaigns.GetActiveBySellerIdAsync(notification.UserId, ct);
        if (active.Count == 0)
            return;

        foreach (var campaign in active)
            campaign.Stop();

        await campaigns.SaveChangesAsync(ct);
    }
}
