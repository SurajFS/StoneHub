namespace Identity.Domain;

public interface ISellerProfileRepository
{
    Task<SellerProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SellerProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    // Profiles with no structured Address yet (legacy free text), tracked for the backfill.
    Task<IReadOnlyList<SellerProfile>> GetWithoutAddressAsync(CancellationToken ct = default);
    void Add(SellerProfile sellerProfile);
    Task SaveChangesAsync(CancellationToken ct = default);
}
