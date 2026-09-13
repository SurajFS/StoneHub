using FluentValidation;

namespace Messaging.Application;

public sealed class StartConversationCommandValidator : AbstractValidator<StartConversationCommand>
{
    public StartConversationCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
