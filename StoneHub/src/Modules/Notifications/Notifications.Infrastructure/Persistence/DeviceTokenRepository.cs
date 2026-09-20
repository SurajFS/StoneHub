using Microsoft.EntityFrameworkCore;
using Notifications.Domain;

namespace Notifications.Infrastructure.Persistence;

public sealed class DeviceTokenRepository(NotificationsDbContext dbContext) : IDeviceTokenRepository
{
    public Task<DeviceToken?> GetByTokenAsync(string expoPushToken, CancellationToken ct = default) =>
        dbContext.DeviceTokens.FirstOrDefaultAsync(x => x.ExpoPushToken == expoPushToken, ct);

    public async Task<IReadOnlyList<DeviceToken>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await dbContext.DeviceTokens.Where(x => x.UserId == userId).ToListAsync(ct);

    public void Add(DeviceToken token) => dbContext.DeviceTokens.Add(token);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
