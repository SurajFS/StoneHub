using Microsoft.EntityFrameworkCore;
using Notifications.Domain;
using Notifications.Infrastructure.Persistence;

namespace Notifications.Infrastructure;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
{
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("notifications");
        builder.ApplyConfiguration(new DeviceTokenConfiguration());
    }
}
