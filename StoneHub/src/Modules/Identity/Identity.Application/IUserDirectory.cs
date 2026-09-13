namespace Identity.Application;

// Display info for any user, resolved from whichever profile they have. Role is inferred from the
// profile type. Published so other modules (e.g. Messaging) can label participants and apply role
// rules without reaching into Identity's tables.
public sealed record UserDirectoryEntry(Guid UserId, string Role, string Name, string? AvatarUrl);

public interface IUserDirectory
{
    Task<UserDirectoryEntry?> GetAsync(Guid userId, CancellationToken ct = default);
}
