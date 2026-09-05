namespace Catalog.Application;

// Minimal product facts other modules need (e.g. Inquiries deriving the wholesaler owner).
public sealed record ProductOwnerInfo(Guid OwnerId, string OwnerType, string Title, bool IsActive);

// Published contract: look up a product's owner + title by id, without exposing Catalog internals.
public interface IProductLookup
{
    Task<ProductOwnerInfo?> GetOwnerInfoAsync(Guid productId, CancellationToken ct = default);
}
