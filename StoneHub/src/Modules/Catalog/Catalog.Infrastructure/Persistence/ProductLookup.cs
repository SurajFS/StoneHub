using Catalog.Application;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public sealed class ProductLookup(CatalogDbContext dbContext) : IProductLookup
{
    public Task<ProductOwnerInfo?> GetOwnerInfoAsync(Guid productId, CancellationToken ct = default) =>
        dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => new ProductOwnerInfo(p.SellerId, p.OwnerType.ToString(), p.Title, p.IsActive))
            .FirstOrDefaultAsync(ct);
}
