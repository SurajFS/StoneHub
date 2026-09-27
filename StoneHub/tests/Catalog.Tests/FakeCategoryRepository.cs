using Catalog.Domain;

namespace Catalog.Tests;

internal sealed class FakeCategoryRepository(params Category[] categories) : ICategoryRepository
{
    private readonly Dictionary<Guid, Category> _categories = categories.ToDictionary(c => c.Id);

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_categories.GetValueOrDefault(id));

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingId = null, CancellationToken ct = default) =>
        Task.FromResult(_categories.Values.Any(c => c.Slug == slug && c.Id != excludingId));

    public void Add(Category category) => _categories[category.Id] = category;

    public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
}
