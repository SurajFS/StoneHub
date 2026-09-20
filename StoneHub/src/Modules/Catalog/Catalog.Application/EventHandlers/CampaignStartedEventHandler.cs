using Advertising.Domain;
using Catalog.Domain;
using MediatR;

namespace Catalog.Application.EventHandlers;

public sealed class CampaignStartedEventHandler(IProductRepository products) : INotificationHandler<CampaignStartedEvent>
{
    public async Task Handle(CampaignStartedEvent notification, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(notification.ProductId, ct);
        if (product is null)
            return;

        product.SetSponsored(true);
        await products.SaveChangesAsync(ct);
    }
}
