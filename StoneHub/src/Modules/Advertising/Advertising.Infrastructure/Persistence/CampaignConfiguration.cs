using Advertising.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Advertising.Infrastructure.Persistence;

public sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("Campaigns");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SellerId).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.StartedAt).IsRequired();

        builder.HasIndex(x => x.SellerId);
        builder.HasIndex(x => x.ProductId);

        builder.Ignore(x => x.DomainEvents);
    }
}
