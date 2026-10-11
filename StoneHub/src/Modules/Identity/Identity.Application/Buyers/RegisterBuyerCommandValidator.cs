using FluentValidation;
using Identity.Application.Profile;

namespace Identity.Application.Buyers;

public sealed class RegisterBuyerCommandValidator : AbstractValidator<RegisterBuyerCommand>
{
    public RegisterBuyerCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        // `!` is safe: the rule only runs when Address is set (see When); it tells the compiler so.
        RuleFor(x => x.Address!).SetValidator(new ProfileAddressInputValidator()).When(x => x.Address is not null);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100).When(x => x.Address is null);
        RuleFor(x => x.BuyerType).IsInEnum();
    }
}
