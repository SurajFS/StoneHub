using Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

public sealed class BuyerProfileRepository(IdentityModuleDbContext dbContext) : IBuyerProfileRepository
{
    public Task<BuyerProfile?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.BuyerProfiles.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<BuyerProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        dbContext.BuyerProfiles.FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public void Add(BuyerProfile buyerProfile) => dbContext.BuyerProfiles.Add(buyerProfile);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
