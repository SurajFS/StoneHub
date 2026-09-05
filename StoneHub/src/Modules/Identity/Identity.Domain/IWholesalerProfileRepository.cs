namespace Identity.Domain;

public interface IWholesalerProfileRepository
{
    Task<WholesalerProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WholesalerProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    void Add(WholesalerProfile wholesalerProfile);
    Task SaveChangesAsync(CancellationToken ct = default);
}
