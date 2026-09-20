using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence;

public sealed class SellerPromotionStatusConfiguration : IEntityTypeConfiguration<SellerPromotionStatus>
{
    public void Configure(EntityTypeBuilder<SellerPromotionStatus> builder)
    {
        builder.ToTable("SellerPromotionStatuses");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IsPremium).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
