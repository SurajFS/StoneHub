namespace Catalog.Domain;

public interface ISellerPromotionStatusRepository
{
    // Upserts — the caller (an event handler) never needs to load-then-save itself.
    Task SetPremiumAsync(Guid sellerId, bool isPremium, CancellationToken ct = default);
}
