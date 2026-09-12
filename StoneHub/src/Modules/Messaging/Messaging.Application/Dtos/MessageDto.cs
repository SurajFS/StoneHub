namespace Messaging.Application.Dtos;

public sealed record MessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string Kind,
    string? Body,
    string? MediaUrl,
    DateTimeOffset SentAt);
