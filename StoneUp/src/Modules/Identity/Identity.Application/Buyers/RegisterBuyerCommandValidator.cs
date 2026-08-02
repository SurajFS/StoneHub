using FluentValidation;

namespace Identity.Application.Buyers;

public sealed class RegisterBuyerCommandValidator : AbstractValidator<RegisterBuyerCommand>
{
    public RegisterBuyerCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BuyerType).IsInEnum();
    }
}
