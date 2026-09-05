using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(120);
        builder.Property(x => x.ParentId);
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.ParentId);

        builder.Ignore(x => x.DomainEvents);

        builder.HasData(
            Seed(CategorySeed.Marble, "Marble", "marble", 1),
            Seed(CategorySeed.Granite, "Granite", "granite", 2),
            Seed(CategorySeed.Tiles, "Tiles", "tiles", 3),
            Seed(CategorySeed.KotaStone, "Kota Stone", "kota-stone", 4),
            Seed(CategorySeed.Handicrafts, "Handicrafts", "handicrafts", 5));
    }

    private static object Seed(Guid id, string name, string slug, int order) => new
    {
        Id = id,
        Name = name,
        Slug = slug,
        ParentId = (Guid?)null,
        DisplayOrder = order,
        IsActive = true
    };
}
