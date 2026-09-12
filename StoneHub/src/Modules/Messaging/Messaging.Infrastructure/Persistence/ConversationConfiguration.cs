using Messaging.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Messaging.Infrastructure.Persistence;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ParticipantAId).IsRequired();
        builder.Property(x => x.ParticipantAName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ParticipantAAvatarUrl).HasMaxLength(500);
        builder.Property(x => x.ParticipantBId).IsRequired();
        builder.Property(x => x.ParticipantBName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ParticipantBAvatarUrl).HasMaxLength(500);
        builder.Property(x => x.ProductTitle).HasMaxLength(200);
        builder.Property(x => x.LastMessagePreview).HasMaxLength(200);
        builder.Property(x => x.CreatedAt).IsRequired();

        // One thread per unordered pair.
        builder.HasIndex(x => new { x.ParticipantAId, x.ParticipantBId }).IsUnique();
        builder.HasIndex(x => x.ParticipantAId);
        builder.HasIndex(x => x.ParticipantBId);

        builder.Ignore(x => x.DomainEvents);
    }
}
