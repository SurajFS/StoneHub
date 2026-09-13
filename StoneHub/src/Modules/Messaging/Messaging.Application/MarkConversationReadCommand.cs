using MediatR;
using SharedKernel;

namespace Messaging.Application;

public sealed record MarkConversationReadCommand(Guid ConversationId, Guid UserId) : IRequest<Result>;
