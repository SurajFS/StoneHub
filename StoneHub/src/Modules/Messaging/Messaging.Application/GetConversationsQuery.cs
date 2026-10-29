using Billing.Application;
using MediatR;
using Messaging.Application.Dtos;

namespace Messaging.Application;

public sealed record GetConversationsQuery(Guid UserId) : IRequest<IReadOnlyList<ConversationDto>>;

// A Free seller/wholesaler still sees who wrote and when (and unread counts), so they know there's
// something waiting, but not what was said: the preview is withheld and each row is marked locked.
public sealed class GetConversationsQueryHandler(IMessagingQueryService queryService, IPlanEntitlements entitlements)
    : IRequestHandler<GetConversationsQuery, IReadOnlyList<ConversationDto>>
{
    public async Task<IReadOnlyList<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken ct)
    {
        var conversations = await queryService.GetConversationsAsync(request.UserId, ct);
        if (await entitlements.CanUseOwnerFeaturesAsync(request.UserId, ct))
            return conversations;

        return conversations.Select(row => row with { LastMessagePreview = null, IsLocked = true }).ToList();
    }
}
