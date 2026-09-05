using Inquiries.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inquiries.Infrastructure.Persistence;

public sealed class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
{
    public void Configure(EntityTypeBuilder<Inquiry> builder)
    {
        builder.ToTable("Inquiries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SellerId).IsRequired();
        builder.Property(x => x.WholesalerId).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.ProductTitle).IsRequired().HasMaxLength(200);
        builder.Property(x => x.SellerName).HasMaxLength(200);
        builder.Property(x => x.Quantity).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Message).HasMaxLength(1000);
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.WholesalerId);
        builder.HasIndex(x => x.SellerId);
        builder.HasIndex(x => x.ProductId);

        builder.Ignore(x => x.DomainEvents);
    }
}
