using Advertising.Domain;
using Catalog.Domain;
using MediatR;

namespace Catalog.Application.EventHandlers;

public sealed class CampaignStoppedEventHandler(IProductRepository products) : INotificationHandler<CampaignStoppedEvent>
{
    public async Task Handle(CampaignStoppedEvent notification, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(notification.ProductId, ct);
        if (product is null)
            return;

        product.SetSponsored(false);
        await products.SaveChangesAsync(ct);
    }
}
