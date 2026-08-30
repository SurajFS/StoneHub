using Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

public sealed class RefreshTokenRepository(IdentityModuleDbContext dbContext) : IRefreshTokenRepository
{
    public void Add(RefreshToken token) => dbContext.RefreshTokens.Add(token);

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default) =>
        dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
