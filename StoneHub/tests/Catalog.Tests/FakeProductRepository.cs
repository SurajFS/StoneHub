using Catalog.Domain;

namespace Catalog.Tests;

// In-memory stand-in so handler authorization paths can be tested without a database.
internal sealed class FakeProductRepository(params Product[] products) : IProductRepository
{
    private readonly Dictionary<Guid, Product> _products = products.ToDictionary(p => p.Id);

    public int SaveCount { get; private set; }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_products.GetValueOrDefault(id));

    public void Add(Product product) => _products[product.Id] = product;

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
