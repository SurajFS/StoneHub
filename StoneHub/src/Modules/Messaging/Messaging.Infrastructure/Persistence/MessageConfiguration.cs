using Messaging.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Messaging.Infrastructure.Persistence;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConversationId).IsRequired();
        builder.Property(x => x.SenderId).IsRequired();
        builder.Property(x => x.Kind).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Body).HasMaxLength(2000);
        builder.Property(x => x.MediaUrl).HasMaxLength(1000);
        builder.Property(x => x.SentAt).IsRequired();

        // Thread reads: ordered within a conversation, and polled by SentAt.
        builder.HasIndex(x => new { x.ConversationId, x.SentAt });

        builder.Ignore(x => x.DomainEvents);
    }
}
