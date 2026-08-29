using FluentValidation;

namespace Identity.Application.Sellers;

public sealed class RegisterSellerCommandValidator : AbstractValidator<RegisterSellerCommand>
{
    public RegisterSellerCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20).Matches(PhonePattern);
        RuleFor(x => x.WhatsAppNumber).MaximumLength(20).Matches(PhonePattern)
            .When(x => !string.IsNullOrWhiteSpace(x.WhatsAppNumber));
    }

    // E.164-ish: optional leading + and 7–15 digits. Kept deliberately permissive so we
    // don't reject valid international formats at registration.
    private const string PhonePattern = @"^\+?[0-9]{7,15}$";
}
