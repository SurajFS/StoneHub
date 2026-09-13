using MediatR;
using Messaging.Application.Dtos;
using Messaging.Domain;
using SharedKernel;

namespace Messaging.Application;

public sealed record SendMessageCommand(
    Guid ConversationId, Guid SenderId, MessageKind Kind, string? Body, string? MediaUrl)
    : IRequest<Result<MessageDto>>;
