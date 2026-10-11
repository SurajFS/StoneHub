using FluentValidation;

namespace Locations.Application.Queries;

public sealed class GetDistrictsQueryValidator : AbstractValidator<GetDistrictsQuery>
{
    public GetDistrictsQueryValidator()
    {
        RuleFor(x => x.RegionCode).NotEmpty().Matches(LocationCodes.RegionCodePattern);
    }
}
