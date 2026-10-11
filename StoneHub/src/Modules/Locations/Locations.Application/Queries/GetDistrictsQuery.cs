using Locations.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Locations.Application.Queries;

public sealed record GetDistrictsQuery(string RegionCode) : IRequest<Result<IReadOnlyList<DistrictDto>>>;
