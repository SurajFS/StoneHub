namespace Catalog.Domain;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    // The live (not deleted) listings owned by this seller/wholesaler user id, tracked, for bulk updates.
    Task<IReadOnlyList<Product>> GetBySellerIdAsync(Guid sellerId, CancellationToken ct = default);
    void Add(Product product);
    Task SaveChangesAsync(CancellationToken ct = default);
}
