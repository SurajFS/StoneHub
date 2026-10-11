using Locations.Domain;
using Locations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Locations.Infrastructure;

public sealed class LocationsDbContext(DbContextOptions<LocationsDbContext> options) : DbContext(options)
{
    public const string Schema = "locations";

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<PostalLocality> PostalLocalities => Set<PostalLocality>();
    public DbSet<DatasetImport> DatasetImports => Set<DatasetImport>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(Schema);
        builder.ApplyConfiguration(new CountryConfiguration());
        builder.ApplyConfiguration(new RegionConfiguration());
        builder.ApplyConfiguration(new DistrictConfiguration());
        builder.ApplyConfiguration(new PostalLocalityConfiguration());
        builder.ApplyConfiguration(new DatasetImportConfiguration());
    }
}
