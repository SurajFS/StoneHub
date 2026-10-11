using Identity.Application;
using Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure;

// Resolves owner display info from whichever profile the user has (seller first, then wholesaler).
// The location label uses Location.Format, the same as OwnerProfileChangedEvent, so a listing's
// label matches whether it was set at creation or refreshed after a profile edit.
public sealed class SellerDirectory(IdentityModuleDbContext dbContext) : ISellerDirectory
{
    public async Task<SellerDirectoryEntry?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var seller = await dbContext.SellerProfiles
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => new { Name = s.CompanyName, s.Location.City, s.Location.State })
            .FirstOrDefaultAsync(ct);
        if (seller is not null)
            return new SellerDirectoryEntry(seller.Name, Location.Format(seller.City, seller.State));

        var wholesaler = await dbContext.WholesalerProfiles
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .Select(w => new { Name = w.BusinessName, w.Location.City, w.Location.State })
            .FirstOrDefaultAsync(ct);
        return wholesaler is null
            ? null
            : new SellerDirectoryEntry(wholesaler.Name, Location.Format(wholesaler.City, wholesaler.State));
    }
}
