using Locations.Application.Dtos;
using Locations.Domain;
using MediatR;
using SharedKernel;

namespace Locations.Application.Queries;

public sealed class LookupPostalCodeQueryHandler(ILocationQueryService queryService)
    : IRequestHandler<LookupPostalCodeQuery, Result<PostalCodeLookupDto>>
{
    // PascalCase like the FluentValidation property names, so clients map every field the same way.
    private const string PostalCodeField = "PostalCode";

    public async Task<Result<PostalCodeLookupDto>> Handle(LookupPostalCodeQuery request, CancellationToken ct)
    {
        var countryCode = LocationCodes.Normalize(request.CountryCode);
        var country = await queryService.GetCountryAsync(countryCode, ct);
        if (country is null)
            return Result.NotFound<PostalCodeLookupDto>("Country not found.");

        var postalCode = request.PostalCode.Trim();
        if (!PostalCodeRule.Matches(country.PostalCodePattern, postalCode))
        {
            return Result.ValidationFailure<PostalCodeLookupDto>(new Dictionary<string, string[]>
            {
                [PostalCodeField] = ["Postal code is not valid for this country."]
            });
        }

        var localities = await queryService.GetPostalLocalitiesAsync(countryCode, postalCode, ct);
        return localities.Count == 0
            ? Result.NotFound<PostalCodeLookupDto>("Postal code not found.")
            : Result.Success(new PostalCodeLookupDto(countryCode, postalCode, localities));
    }
}
