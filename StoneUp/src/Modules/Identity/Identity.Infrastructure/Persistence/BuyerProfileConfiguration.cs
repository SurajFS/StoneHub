using Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence;

public sealed class BuyerProfileConfiguration : IEntityTypeConfiguration<BuyerProfile>
{
    public void Configure(EntityTypeBuilder<BuyerProfile> builder)
    {
        builder.ToTable("BuyerProfiles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.BuyerType).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.City).IsRequired().HasMaxLength(100);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.Ignore(x => x.DomainEvents);
    }
}
