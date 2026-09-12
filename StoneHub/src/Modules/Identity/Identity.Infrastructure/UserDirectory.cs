using Identity.Application;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure;

// Resolves display info + role for any user by probing each profile table in turn. Each user has
// exactly one profile, so the first match is authoritative.
public sealed class UserDirectory(IdentityModuleDbContext dbContext) : IUserDirectory
{
    public async Task<UserDirectoryEntry?> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var seller = await dbContext.SellerProfiles
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => new UserDirectoryEntry(userId, "Seller", s.CompanyName, s.AvatarUrl))
            .FirstOrDefaultAsync(ct);
        if (seller is not null)
            return seller;

        var wholesaler = await dbContext.WholesalerProfiles
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .Select(w => new UserDirectoryEntry(userId, "Wholesaler", w.BusinessName, w.AvatarUrl))
            .FirstOrDefaultAsync(ct);
        if (wholesaler is not null)
            return wholesaler;

        return await dbContext.BuyerProfiles
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .Select(b => new UserDirectoryEntry(userId, "Buyer", b.DisplayName, b.AvatarUrl))
            .FirstOrDefaultAsync(ct);
    }
}
