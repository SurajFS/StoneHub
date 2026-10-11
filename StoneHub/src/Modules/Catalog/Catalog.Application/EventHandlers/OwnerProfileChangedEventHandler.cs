using Catalog.Domain;
using Identity.Domain;
using MediatR;

namespace Catalog.Application.EventHandlers;

// Keeps the owner name + location copied onto listings in step with the owner's profile. Before
// this, a profile edit left every existing listing showing (and searchable by) the old values.
public sealed class OwnerProfileChangedEventHandler(IProductRepository products)
    : INotificationHandler<OwnerProfileChangedEvent>
{
    public async Task Handle(OwnerProfileChangedEvent notification, CancellationToken ct)
    {
        var listings = await products.GetBySellerIdAsync(notification.UserId, ct);
        if (listings.Count == 0)
            return;

        foreach (var listing in listings)
            listing.SetSellerInfo(notification.Name, notification.Location);

        await products.SaveChangesAsync(ct);
    }
}
