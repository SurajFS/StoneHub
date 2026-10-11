namespace Identity.Domain;

public interface IWholesalerProfileRepository
{
    Task<WholesalerProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WholesalerProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    // Profiles with no structured Address yet (legacy free text), tracked for the backfill.
    Task<IReadOnlyList<WholesalerProfile>> GetWithoutAddressAsync(CancellationToken ct = default);
    void Add(WholesalerProfile wholesalerProfile);
    Task SaveChangesAsync(CancellationToken ct = default);
}
