using Advertising.Domain;
using Billing.Domain;
using MediatR;

namespace Advertising.Application.EventHandlers;

// "Automatic Downgrade... all Premium benefits should be disabled" — active campaigns are a
// Premium benefit, so they stop the moment the subscription does.
public sealed class SubscriptionExpiredEventHandler(ICampaignRepository campaigns)
    : INotificationHandler<SubscriptionExpiredEvent>
{
    public async Task Handle(SubscriptionExpiredEvent notification, CancellationToken ct)
    {
        var active = await campaigns.GetActiveBySellerIdAsync(notification.UserId, ct);
        if (active.Count == 0)
            return;

        foreach (var campaign in active)
            campaign.Stop();

        await campaigns.SaveChangesAsync(ct);
    }
}
