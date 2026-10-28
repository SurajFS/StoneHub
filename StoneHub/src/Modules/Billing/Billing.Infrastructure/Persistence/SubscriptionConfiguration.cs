using Billing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Persistence;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    private const int GatewayIdMaxLength = 100;
    private const int CheckoutUrlMaxLength = 500;

    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Plan).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.PaymentNotes).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.GatewaySubscriptionId).HasMaxLength(GatewayIdMaxLength);
        builder.Property(x => x.WillRenew).IsRequired();
        builder.Property(x => x.CheckoutUrl).HasMaxLength(CheckoutUrlMaxLength);

        // One subscription per user.
        builder.HasIndex(x => x.UserId).IsUnique();

        // Webhooks find the subscription by the gateway's id.
        builder.HasIndex(x => x.GatewaySubscriptionId).IsUnique();

        builder.Ignore(x => x.DomainEvents);
    }
}
