using MediatR;
using Messaging.Application.Dtos;
using Messaging.Domain;
using SharedKernel;

namespace Messaging.Application;

public sealed record GetMessagesQuery(Guid ConversationId, Guid UserId, DateTimeOffset? Since)
    : IRequest<Result<IReadOnlyList<MessageDto>>>;

public sealed class GetMessagesQueryHandler(
    IConversationRepository conversations,
    IMessagingQueryService queryService) : IRequestHandler<GetMessagesQuery, Result<IReadOnlyList<MessageDto>>>
{
    private const int PageSize = 50;

    public async Task<Result<IReadOnlyList<MessageDto>>> Handle(GetMessagesQuery request, CancellationToken ct)
    {
        var conversation = await conversations.GetByIdAsync(request.ConversationId, ct);
        if (conversation is null)
            return Result.NotFound<IReadOnlyList<MessageDto>>("Conversation not found.");
        if (!conversation.HasParticipant(request.UserId))
            return Result.Forbidden<IReadOnlyList<MessageDto>>("You are not part of this conversation.");

        var messages = await queryService.GetMessagesAsync(request.ConversationId, request.Since, PageSize, ct);
        return Result.Success(messages);
    }
}
