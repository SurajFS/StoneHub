using Catalog.Application;
using Catalog.Application.Dtos;
using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Catalog.Infrastructure.Persistence;

public sealed class ProductQueryService(CatalogDbContext dbContext) : IProductQueryService
{
    // Queries bypass the FluentValidation pipeline, so paging inputs are clamped here to
    // avoid a caller requesting page 0 or an unbounded page size.
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(ProjectToDto())
            .FirstOrDefaultAsync(ct);

    public async Task<PagedResult<ProductDto>> SearchAsync(ProductSearchFilter filter, CancellationToken ct = default)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize <= 0 ? DefaultPageSize : filter.PageSize, 1, MaxPageSize);

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

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ProjectToDto())
            .ToListAsync(ct);

        return new PagedResult<ProductDto>(items, total, page, pageSize);
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
