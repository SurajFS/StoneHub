using System.Linq.Expressions;
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

        // Search only surfaces active listings; owners see their own inactive ones via GetBySeller.
        var query = dbContext.Products.AsNoTracking().Where(p => p.IsActive);

        if (filter.CategoryId is Guid categoryId)
            query = query.Where(p => p.CategoryId == categoryId);

        if (filter.SubcategoryId is Guid subcategoryId)
            query = query.Where(p => p.SubcategoryId == subcategoryId);

        // Partial keyword match across title, tags, and seller name ("Krishna" → "Krishna Idol").
        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var pattern = $"%{filter.Keyword.Trim()}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.Title, pattern) ||
                (p.SellerName != null && EF.Functions.ILike(p.SellerName, pattern)) ||
                p.Tags.Any(tag => EF.Functions.ILike(tag, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(filter.SellerName))
            query = query.Where(p =>
                p.SellerName != null && EF.Functions.ILike(p.SellerName, $"%{filter.SellerName.Trim()}%"));

        if (!string.IsNullOrWhiteSpace(filter.Location))
            query = query.Where(p =>
                p.SellerLocation != null && EF.Functions.ILike(p.SellerLocation, $"%{filter.Location.Trim()}%"));

        if (!string.IsNullOrWhiteSpace(filter.OwnerType) &&
            Enum.TryParse<ListingOwnerType>(filter.OwnerType, ignoreCase: true, out var ownerType))
            query = query.Where(p => p.OwnerType == ownerType);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price.Amount >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price.Amount <= filter.MaxPrice.Value);

        if (filter.InStockOnly)
            query = query.Where(p => p.IsAvailable);

        // Wholesale buyers filtering to listings they can meet the minimum order for.
        if (filter.MaxMinimumOrderQuantity.HasValue)
            query = query.Where(p =>
                p.MinimumOrderQuantity != null && p.MinimumOrderQuantity <= filter.MaxMinimumOrderQuantity.Value);

        var total = await query.CountAsync(ct);

        query = filter.SortBy switch
        {
            "price_asc" => query.OrderBy(p => p.Price.Amount),
            "price_desc" => query.OrderByDescending(p => p.Price.Amount),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var items = await query
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

    private static Expression<Func<Product, ProductDto>> ProjectToDto() => p => new ProductDto(
        p.Id,
        p.SellerId,
        p.OwnerType.ToString(),
        p.Title,
        p.CategoryId,
        p.SubcategoryId,
        p.Size,
        p.Thickness,
        p.Finish,
        p.Color,
        p.Unit.ToString(),
        p.Tags,
        p.QuantityAvailable,
        p.Price.Amount,
        p.Price.Currency,
        p.WholesalePrice,
        p.MinimumOrderQuantity,
        p.SellerName,
        p.SellerLocation,
        p.IsAvailable,
        p.IsActive,
        p.Media.Select(m => m.Url).ToList(),
        p.CreatedAt);
}
