using Locations.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Locations.Application.Queries;

public sealed class GetDistrictsQueryHandler(ILocationQueryService queryService)
    : IRequestHandler<GetDistrictsQuery, Result<IReadOnlyList<DistrictDto>>>
{
    public async Task<Result<IReadOnlyList<DistrictDto>>> Handle(GetDistrictsQuery request, CancellationToken ct)
    {
        var regionCode = LocationCodes.Normalize(request.RegionCode);
        if (!await queryService.RegionExistsAsync(regionCode, ct))
            return Result.NotFound<IReadOnlyList<DistrictDto>>("Region not found.");

        return Result.Success(await queryService.GetDistrictsAsync(regionCode, ct));
    }
}
