using Locations.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locations.Infrastructure.Persistence;

public sealed class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    public const string Table = "Districts";
    public const int CodeMaxLength = 100;

    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.ToTable(Table);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasMaxLength(CodeMaxLength);
        builder.Property(x => x.RegionId).IsRequired().HasMaxLength(6);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

        builder.HasOne<Region>().WithMany().HasForeignKey(x => x.RegionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.RegionId);
    }
}
