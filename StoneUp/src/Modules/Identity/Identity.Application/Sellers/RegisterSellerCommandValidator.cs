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
    }
}
