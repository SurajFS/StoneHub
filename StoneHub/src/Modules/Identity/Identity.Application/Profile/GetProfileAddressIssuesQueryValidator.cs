using FluentValidation;

namespace Identity.Application.Profile;

public sealed class GetProfileAddressIssuesQueryValidator : AbstractValidator<GetProfileAddressIssuesQuery>
{
    private static readonly string[] Roles =
        [AddressIssueRules.SellerRole, AddressIssueRules.WholesalerRole, AddressIssueRules.BuyerRole];

    public GetProfileAddressIssuesQueryValidator()
    {
        RuleFor(x => x.Role)
            .Must(role => Roles.Contains(role))
            .When(x => x.Role is not null)
            .WithMessage($"Role must be one of: {string.Join(", ", Roles)}.");
    }
}
