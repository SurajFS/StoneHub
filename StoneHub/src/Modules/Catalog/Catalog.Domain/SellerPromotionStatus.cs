using SharedKernel;

namespace Catalog.Domain;

// Denormalized read model: whether a seller currently has an active Premium subscription.
// Kept in sync via Billing's published domain events (SubscriptionActivated/Expired/Cancelled)
// so search/ranking never reaches into Billing's own tables. Absence of a row means "not
// Premium" — most sellers never need one created.
public sealed class SellerPromotionStatus : Entity<Guid>
{
    public bool IsPremium { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private SellerPromotionStatus() { }

    private SellerPromotionStatus(Guid sellerId, bool isPremium) : base(sellerId)
    {
        IsPremium = isPremium;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static SellerPromotionStatus Create(Guid sellerId, bool isPremium) => new(sellerId, isPremium);

    public void SetPremium(bool isPremium)
    {
        IsPremium = isPremium;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
