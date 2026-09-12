using FluentValidation;
using Messaging.Domain;

namespace Messaging.Application;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.Body).MaximumLength(2000);
        RuleFor(x => x.MediaUrl).MaximumLength(1000);

        // A text message needs a body; an image message needs a media URL (caption optional).
        RuleFor(x => x.Body).NotEmpty().When(x => x.Kind == MessageKind.Text)
            .WithMessage("Message cannot be empty.");
        RuleFor(x => x.MediaUrl).NotEmpty().When(x => x.Kind == MessageKind.Image)
            .WithMessage("An image message needs a media URL.");
    }
}
