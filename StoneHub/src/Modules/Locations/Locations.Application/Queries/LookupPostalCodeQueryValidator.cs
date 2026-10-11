using FluentValidation;

namespace Locations.Application.Queries;

public sealed class LookupPostalCodeQueryValidator : AbstractValidator<LookupPostalCodeQuery>
{
    public LookupPostalCodeQueryValidator()
    {
        RuleFor(x => x.CountryCode).NotEmpty().Matches(LocationCodes.CountryCodePattern);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(LocationCodes.MaxPostalCodeLength);
    }
}
