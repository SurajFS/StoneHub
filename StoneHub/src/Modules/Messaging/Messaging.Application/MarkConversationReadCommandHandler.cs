using MediatR;
using Messaging.Domain;
using SharedKernel;

namespace Messaging.Application;

public sealed class MarkConversationReadCommandHandler(IConversationRepository conversations)
    : IRequestHandler<MarkConversationReadCommand, Result>
{
    public async Task<Result> Handle(MarkConversationReadCommand request, CancellationToken ct)
    {
        var conversation = await conversations.GetByIdAsync(request.ConversationId, ct);
        if (conversation is null)
            return Result.NotFound("Conversation not found.");
        if (!conversation.HasParticipant(request.UserId))
            return Result.Forbidden("You are not part of this conversation.");

        conversation.MarkRead(request.UserId, DateTimeOffset.UtcNow);
        await conversations.SaveChangesAsync(ct);
        return Result.Success();
    }
}
