using Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

public sealed class WholesalerProfileRepository(IdentityModuleDbContext dbContext) : IWholesalerProfileRepository
{
    public Task<WholesalerProfile?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.WholesalerProfiles.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<WholesalerProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        dbContext.WholesalerProfiles.FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public void Add(WholesalerProfile wholesalerProfile) => dbContext.WholesalerProfiles.Add(wholesalerProfile);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
