using MediatR;
using Messaging.Application.Dtos;
using Messaging.Domain;
using SharedKernel;

namespace Messaging.Application;

public sealed class SendMessageCommandHandler(
    IConversationRepository conversations,
    IMessageRepository messages) : IRequestHandler<SendMessageCommand, Result<MessageDto>>
{
    public async Task<Result<MessageDto>> Handle(SendMessageCommand request, CancellationToken ct)
    {
        var conversation = await conversations.GetByIdAsync(request.ConversationId, ct);
        if (conversation is null)
            return Result.NotFound<MessageDto>("Conversation not found.");
        if (!conversation.HasParticipant(request.SenderId))
            return Result.Forbidden<MessageDto>("You are not part of this conversation.");

        var result = Message.Create(request.ConversationId, request.SenderId, request.Kind, request.Body, request.MediaUrl);
        if (result.IsFailure)
            return Result.Failure<MessageDto>(result);

        var message = result.Value;
        messages.Add(message);
        // List preview: the caption if any, otherwise a placeholder for the attachment.
        conversation.RecordMessage(message.Body ?? "Photo", message.SentAt);
        // The new message and the conversation's last-message fields share one DbContext, so a
        // single SaveChanges commits both in the same transaction.
        await conversations.SaveChangesAsync(ct);

        return Result.Success(new MessageDto(
            message.Id, message.ConversationId, message.SenderId,
            message.Kind.ToString(), message.Body, message.MediaUrl, message.SentAt));
    }
}
