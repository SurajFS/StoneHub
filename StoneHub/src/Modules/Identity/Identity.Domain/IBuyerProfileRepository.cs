namespace Identity.Domain;

public interface IBuyerProfileRepository
{
    Task<BuyerProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<BuyerProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    void Add(BuyerProfile buyerProfile);
    Task SaveChangesAsync(CancellationToken ct = default);
}
