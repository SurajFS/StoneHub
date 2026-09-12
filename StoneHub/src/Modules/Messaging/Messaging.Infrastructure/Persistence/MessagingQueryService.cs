using Messaging.Application;
using Messaging.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Messaging.Infrastructure.Persistence;

public sealed class MessagingQueryService(MessagingDbContext dbContext) : IMessagingQueryService
{
    public async Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(Guid userId, CancellationToken ct = default) =>
        await dbContext.Conversations
            .AsNoTracking()
            .Where(c => c.ParticipantAId == userId || c.ParticipantBId == userId)
            .OrderByDescending(c => c.LastMessageAt)
            .Select(c => new ConversationDto(
                c.Id,
                c.ParticipantAId == userId ? c.ParticipantBId : c.ParticipantAId,
                c.ParticipantAId == userId ? c.ParticipantBName : c.ParticipantAName,
                c.ParticipantAId == userId ? c.ParticipantBAvatarUrl : c.ParticipantAAvatarUrl,
                c.ProductId,
                c.ProductTitle,
                c.LastMessagePreview,
                c.LastMessageAt,
                dbContext.Messages.Count(m =>
                    m.ConversationId == c.Id &&
                    m.SenderId != userId &&
                    ((c.ParticipantAId == userId ? c.ParticipantALastReadAt : c.ParticipantBLastReadAt) == null ||
                     m.SentAt > (c.ParticipantAId == userId ? c.ParticipantALastReadAt : c.ParticipantBLastReadAt)))))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MessageDto>> GetMessagesAsync(
        Guid conversationId, DateTimeOffset? since, int limit, CancellationToken ct = default)
    {
        var query = dbContext.Messages.AsNoTracking().Where(m => m.ConversationId == conversationId);

        if (since is not null)
        {
            return await query
                .Where(m => m.SentAt > since)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageDto(m.Id, m.ConversationId, m.SenderId, m.Kind.ToString(), m.Body, m.MediaUrl, m.SentAt))
                .ToListAsync(ct);
        }

        // Initial load: the most recent page, returned oldest→newest for display.
        var recent = await query
            .OrderByDescending(m => m.SentAt)
            .Take(limit)
            .Select(m => new MessageDto(m.Id, m.ConversationId, m.SenderId, m.Kind.ToString(), m.Body, m.MediaUrl, m.SentAt))
            .ToListAsync(ct);

        return recent.OrderBy(m => m.SentAt).ToList();
    }
}
