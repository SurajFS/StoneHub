using FluentValidation;
using Identity.Domain;

namespace Identity.Application.Profile;

// Shape rules only. Whether the codes exist and fit together is checked against the Locations
// reference data by IProfileLocationResolver; which fields a role must fill is checked there too.
public sealed class ProfileAddressInputValidator : AbstractValidator<ProfileAddressInput>
{
    public ProfileAddressInputValidator()
    {
        RuleFor(x => x.CountryCode).NotEmpty().MaximumLength(Address.CodeMaxLength);
        RuleFor(x => x.RegionCode).NotEmpty().MaximumLength(Address.CodeMaxLength);
        RuleFor(x => x.DistrictCode).NotEmpty().MaximumLength(Address.CodeMaxLength);
        RuleFor(x => x.Line1).MaximumLength(Address.LineMaxLength);
        RuleFor(x => x.Line2).MaximumLength(Address.LineMaxLength);
        RuleFor(x => x.PostalCode).MaximumLength(Address.PostalCodeMaxLength);
        RuleFor(x => x.Locality).MaximumLength(Address.LocalityMaxLength);
    }
}
