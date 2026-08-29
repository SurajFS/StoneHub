using Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence;

public sealed class SellerProfileConfiguration : IEntityTypeConfiguration<SellerProfile>
{
    public void Configure(EntityTypeBuilder<SellerProfile> builder)
    {
        builder.ToTable("SellerProfiles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Phone).IsRequired().HasMaxLength(20);
        builder.Property(x => x.WhatsAppNumber).HasMaxLength(20);
        builder.Property(x => x.IsVerified).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.OwnsOne(x => x.Location, location =>
        {
            location.Property(l => l.City).HasColumnName("City").IsRequired().HasMaxLength(100);
            location.Property(l => l.State).HasColumnName("State").IsRequired().HasMaxLength(100);
        });

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.Ignore(x => x.DomainEvents);
    }
}
