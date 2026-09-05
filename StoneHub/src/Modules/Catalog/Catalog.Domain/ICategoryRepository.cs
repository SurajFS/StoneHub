namespace Catalog.Domain;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludingId = null, CancellationToken ct = default);
    void Add(Category category);
    Task SaveChangesAsync(CancellationToken ct = default);
}
