using FluentValidation;
using Identity.Application.Profile;

namespace Identity.Application.Wholesalers;

public sealed class RegisterWholesalerCommandValidator : AbstractValidator<RegisterWholesalerCommand>
{
    public RegisterWholesalerCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(200);
        // `!` is safe: the rule only runs when Address is set (see When); it tells the compiler so.
        RuleFor(x => x.Address!).SetValidator(new ProfileAddressInputValidator()).When(x => x.Address is not null);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100).When(x => x.Address is null);
        RuleFor(x => x.State).NotEmpty().MaximumLength(100).When(x => x.Address is null);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20).Matches(PhonePattern);
        RuleFor(x => x.WhatsAppNumber).MaximumLength(20).Matches(PhonePattern)
            .When(x => !string.IsNullOrWhiteSpace(x.WhatsAppNumber));
    }

    // E.164-ish: optional leading + and 7–15 digits (mirrors the seller registration rule).
    private const string PhonePattern = @"^\+?[0-9]{7,15}$";
}
