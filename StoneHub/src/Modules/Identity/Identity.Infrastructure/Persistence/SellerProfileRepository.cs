using Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

public sealed class SellerProfileRepository(IdentityModuleDbContext dbContext) : ISellerProfileRepository
{
    public Task<SellerProfile?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.SellerProfiles.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<SellerProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        dbContext.SellerProfiles.FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public async Task<IReadOnlyList<SellerProfile>> GetWithoutAddressAsync(CancellationToken ct = default) =>
        await dbContext.SellerProfiles.Where(x => x.Address == null).ToListAsync(ct);

    public void Add(SellerProfile sellerProfile) => dbContext.SellerProfiles.Add(sellerProfile);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
