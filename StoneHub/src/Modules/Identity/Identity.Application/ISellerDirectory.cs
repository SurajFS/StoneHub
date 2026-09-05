namespace Identity.Application;

// Owner display info for a user, resolved from either a seller or a wholesaler profile.
public sealed record SellerDirectoryEntry(string Name, string? Location);

// Published contract: lets other modules (e.g. Catalog) look up a seller/wholesaler's display
// name + location by user id, without reaching into Identity's tables. Consumed via DI.
public interface ISellerDirectory
{
    Task<SellerDirectoryEntry?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
