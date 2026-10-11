using Locations.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Locations.Application.Queries;

public sealed class GetRegionsQueryHandler(ILocationQueryService queryService)
    : IRequestHandler<GetRegionsQuery, Result<IReadOnlyList<RegionDto>>>
{
    public async Task<Result<IReadOnlyList<RegionDto>>> Handle(GetRegionsQuery request, CancellationToken ct)
    {
        var countryCode = LocationCodes.Normalize(request.CountryCode);
        var country = await queryService.GetCountryAsync(countryCode, ct);
        if (country is null)
            return Result.NotFound<IReadOnlyList<RegionDto>>("Country not found.");

        return Result.Success(await queryService.GetRegionsAsync(countryCode, ct));
    }
}
