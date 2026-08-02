using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SellerId).IsRequired();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.MaterialType).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Size).HasMaxLength(100);
        builder.Property(x => x.Thickness).HasMaxLength(50);
        builder.Property(x => x.Finish).HasMaxLength(50);
        builder.Property(x => x.QuantityAvailable).HasColumnType("numeric(18,2)");
        builder.Property(x => x.IsAvailable).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.OwnsOne(x => x.Price, price =>
        {
            price.Property(p => p.Amount).HasColumnName("Price").HasColumnType("numeric(18,2)");
            price.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        builder.OwnsMany(x => x.Media, media =>
        {
            media.ToTable("ProductMedia");
            media.WithOwner().HasForeignKey("ProductId");
            media.Property<int>("Id");
            media.HasKey("Id");
            media.Property(m => m.Url).IsRequired().HasMaxLength(2048);
            media.Property(m => m.MediaType).HasConversion<string>().HasMaxLength(20);
        });

        builder.Navigation(x => x.Media).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.SellerId);
        builder.HasIndex(x => x.MaterialType);

        builder.Ignore(x => x.DomainEvents);
    }
}
