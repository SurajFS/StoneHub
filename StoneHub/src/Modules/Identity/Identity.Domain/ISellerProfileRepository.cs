namespace Identity.Domain;

public interface ISellerProfileRepository
{
    Task<SellerProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SellerProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    void Add(SellerProfile sellerProfile);
    Task SaveChangesAsync(CancellationToken ct = default);
}
