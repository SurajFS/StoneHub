namespace Catalog.Domain;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Add(Product product);
    Task SaveChangesAsync(CancellationToken ct = default);
}
