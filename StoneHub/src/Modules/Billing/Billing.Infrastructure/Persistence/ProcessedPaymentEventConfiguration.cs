using Billing.Application.Payments;
using Billing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Persistence;

public sealed class ProcessedPaymentEventConfiguration : IEntityTypeConfiguration<ProcessedPaymentEvent>
{
    private const int EventTypeMaxLength = 100;

    public void Configure(EntityTypeBuilder<ProcessedPaymentEvent> builder)
    {
        builder.ToTable("ProcessedPaymentEvents");

        // The gateway's event id is the key, so a concurrent duplicate delivery fails the insert
        // (and is retried by the gateway, then seen as a duplicate) instead of applying twice.
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("EventId").HasMaxLength(HandlePaymentWebhookCommandValidator.MaxEventIdLength);
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(EventTypeMaxLength);
        builder.Property(x => x.ProcessedAt).IsRequired();
    }
}
