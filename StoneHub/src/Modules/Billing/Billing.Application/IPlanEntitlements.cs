namespace Billing.Application;

// Published so Catalog, Messaging and Inquiries can ask "may this user use the owner features?"
// without reaching into Billing's tables. Owner features: adding listings, and reading or sending
// messages and inquiries.
public interface IPlanEntitlements
{
    // Buyers and admins always may. Sellers and Wholesalers need an active paid plan (Basic or
    // Premium).
    Task<bool> CanUseOwnerFeaturesAsync(Guid userId, CancellationToken ct = default);
}
