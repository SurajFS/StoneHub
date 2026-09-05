using Identity.Application;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure;

// Resolves owner display info from whichever profile the user has (seller first, then wholesaler).
public sealed class SellerDirectory(IdentityModuleDbContext dbContext) : ISellerDirectory
{
    public async Task<SellerDirectoryEntry?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var seller = await dbContext.SellerProfiles
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => new SellerDirectoryEntry(s.CompanyName, s.Location.City + ", " + s.Location.State))
            .FirstOrDefaultAsync(ct);
        if (seller is not null)
            return seller;

        return await dbContext.WholesalerProfiles
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .Select(w => new SellerDirectoryEntry(w.BusinessName, w.Location.City + ", " + w.Location.State))
            .FirstOrDefaultAsync(ct);
    }
}
