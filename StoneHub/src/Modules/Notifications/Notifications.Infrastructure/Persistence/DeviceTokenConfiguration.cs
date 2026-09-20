using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notifications.Domain;

namespace Notifications.Infrastructure.Persistence;

public sealed class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.ToTable("DeviceTokens");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.ExpoPushToken).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Platform).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.ExpoPushToken).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}
