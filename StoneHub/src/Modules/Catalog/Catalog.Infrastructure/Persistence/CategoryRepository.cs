using Catalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public sealed class CategoryRepository(CatalogDbContext dbContext) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingId = null, CancellationToken ct = default) =>
        dbContext.Categories.AnyAsync(x => x.Slug == slug && (excludingId == null || x.Id != excludingId), ct);

    public void Add(Category category) => dbContext.Categories.Add(category);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
