using Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public sealed class SellerPromotionStatusRepository(CatalogDbContext dbContext) : ISellerPromotionStatusRepository
{
    public async Task SetPremiumAsync(Guid sellerId, bool isPremium, CancellationToken ct = default)
    {
        var existing = await dbContext.SellerPromotionStatuses.FirstOrDefaultAsync(x => x.Id == sellerId, ct);
        if (existing is null)
            dbContext.SellerPromotionStatuses.Add(SellerPromotionStatus.Create(sellerId, isPremium));
        else
            existing.SetPremium(isPremium);

        await dbContext.SaveChangesAsync(ct);
    }
}
