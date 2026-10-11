using Locations.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locations.Infrastructure.Persistence;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasMaxLength(2);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.HasRegions).IsRequired();
        builder.Property(x => x.PostalCodePattern).HasMaxLength(100);

        builder.HasData(new
        {
            Id = CountrySeed.India,
            Name = CountrySeed.IndiaName,
            HasRegions = true,
            PostalCodePattern = CountrySeed.IndiaPostalCodePattern
        });
    }
}
