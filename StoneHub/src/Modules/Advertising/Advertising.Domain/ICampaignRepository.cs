namespace Advertising.Domain;

public interface ICampaignRepository
{
    Task<Campaign?> GetByIdAsync(Guid id, CancellationToken ct = default);

    // Guards against starting a second campaign for the same product.
    Task<Campaign?> GetActiveByProductIdAsync(Guid productId, CancellationToken ct = default);

    Task<IReadOnlyList<Campaign>> GetBySellerIdAsync(Guid sellerId, CancellationToken ct = default);

    // For auto-stopping everything when a seller's Premium subscription ends.
    Task<IReadOnlyList<Campaign>> GetActiveBySellerIdAsync(Guid sellerId, CancellationToken ct = default);

    void Add(Campaign campaign);

    Task SaveChangesAsync(CancellationToken ct = default);
}
