using MediatR;
using Messaging.Application.Dtos;

namespace Messaging.Application;

public sealed record GetConversationsQuery(Guid UserId) : IRequest<IReadOnlyList<ConversationDto>>;

public sealed class GetConversationsQueryHandler(IMessagingQueryService queryService)
    : IRequestHandler<GetConversationsQuery, IReadOnlyList<ConversationDto>>
{
    public Task<IReadOnlyList<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken ct) =>
        queryService.GetConversationsAsync(request.UserId, ct);
}
