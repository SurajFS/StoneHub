using Locations.Application.Dtos;
using MediatR;

namespace Locations.Application.Queries;

public sealed class GetCountriesQueryHandler(ILocationQueryService queryService)
    : IRequestHandler<GetCountriesQuery, IReadOnlyList<CountryDto>>
{
    public Task<IReadOnlyList<CountryDto>> Handle(GetCountriesQuery request, CancellationToken ct) =>
        queryService.GetCountriesAsync(ct);
}
