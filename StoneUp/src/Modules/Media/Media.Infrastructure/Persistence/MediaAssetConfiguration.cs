using Media.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Media.Infrastructure.Persistence;

public sealed class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("MediaAssets");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OwnerId).IsRequired();
        builder.Property(x => x.Key).IsRequired().HasMaxLength(512);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Url).IsRequired().HasMaxLength(1024);
        builder.Property(x => x.Status).HasConversion<string>().IsRequired().HasMaxLength(20);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.OwnerId);

        builder.Ignore(x => x.DomainEvents);
    }
}
