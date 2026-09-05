using Catalog.Application;
using Catalog.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public sealed class CategoryQueryService(CatalogDbContext dbContext) : ICategoryQueryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(bool includeInactive, CancellationToken ct = default)
    {
        var query = dbContext.Categories.AsNoTracking().AsQueryable();

        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.ParentId != null) // top-level (false) before subcategories (true)
            .ThenBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Slug, c.ParentId, c.DisplayOrder, c.IsActive))
            .ToListAsync(ct);
    }
}
