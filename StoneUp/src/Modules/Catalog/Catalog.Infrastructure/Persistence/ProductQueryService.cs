using Catalog.Application;
using Catalog.Application.Dtos;
using Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public sealed class ProductQueryService(CatalogDbContext dbContext) : IProductQueryService
{
    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(ProjectToDto())
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<ProductDto>> SearchAsync(ProductSearchFilter filter, CancellationToken ct = default)
    {
        var query = dbContext.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.MaterialType) &&
            Enum.TryParse<MaterialType>(filter.MaterialType, ignoreCase: true, out var materialType))
        {
            query = query.Where(p => p.MaterialType == materialType);
        }

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            query = query.Where(p => p.Title.Contains(filter.Keyword));

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price.Amount >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price.Amount <= filter.MaxPrice.Value);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(ProjectToDto())
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductDto>> GetBySellerAsync(Guid sellerId, CancellationToken ct = default) =>
        await dbContext.Products
            .AsNoTracking()
            .Where(p => p.SellerId == sellerId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(ProjectToDto())
            .ToListAsync(ct);

    private static System.Linq.Expressions.Expression<Func<Product, ProductDto>> ProjectToDto() => p => new ProductDto(
        p.Id,
        p.SellerId,
        p.Title,
        p.MaterialType.ToString(),
        p.Size,
        p.Thickness,
        p.Finish,
        p.QuantityAvailable,
        p.Price.Amount,
        p.Price.Currency,
        p.IsAvailable,
        p.Media.Select(m => m.Url).ToList(),
        p.CreatedAt);
}
