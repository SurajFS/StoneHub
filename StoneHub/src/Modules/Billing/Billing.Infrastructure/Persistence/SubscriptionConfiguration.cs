using Billing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Persistence;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
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

        // One subscription per user.
        builder.HasIndex(x => x.UserId).IsUnique();

        builder.Ignore(x => x.DomainEvents);
    }
}
