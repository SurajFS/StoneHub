using Locations.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locations.Infrastructure.Persistence;

public sealed class PostalLocalityConfiguration : IEntityTypeConfiguration<PostalLocality>
{
    public const string Table = "PostalLocalities";

    public void Configure(EntityTypeBuilder<PostalLocality> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.CountryId).IsRequired().HasMaxLength(2);
        builder.Property(x => x.PostalCode).IsRequired().HasMaxLength(12);
        builder.Property(x => x.DistrictId).IsRequired().HasMaxLength(DistrictConfiguration.CodeMaxLength);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);

        builder.HasOne<Country>().WithMany().HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<District>().WithMany().HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.Restrict);

        // Lookup path is (country, postal code); the unique index also guards the importer's dedupe.
        builder.HasIndex(x => new { x.CountryId, x.PostalCode, x.DistrictId, x.Name }).IsUnique();
        builder.HasIndex(x => x.DistrictId);
    }
}
