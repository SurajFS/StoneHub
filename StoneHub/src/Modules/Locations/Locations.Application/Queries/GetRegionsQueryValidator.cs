using FluentValidation;

namespace Locations.Application.Queries;

public sealed class GetRegionsQueryValidator : AbstractValidator<GetRegionsQuery>
{
    public GetRegionsQueryValidator()
    {
        RuleFor(x => x.CountryCode).NotEmpty().Matches(LocationCodes.CountryCodePattern);
    }
}
