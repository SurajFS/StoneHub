using Advertising.Domain;
using Advertising.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Advertising.Infrastructure;

public sealed class AdvertisingDbContext(DbContextOptions<AdvertisingDbContext> options) : DbContext(options)
{
    public DbSet<Campaign> Campaigns => Set<Campaign>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("advertising");
        builder.ApplyConfiguration(new CampaignConfiguration());
    }
}
