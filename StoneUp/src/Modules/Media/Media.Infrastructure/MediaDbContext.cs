using Media.Domain;
using Media.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Media.Infrastructure;

public sealed class MediaDbContext(DbContextOptions<MediaDbContext> options) : DbContext(options)
{
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("media");
        builder.ApplyConfiguration(new MediaAssetConfiguration());
    }
}
