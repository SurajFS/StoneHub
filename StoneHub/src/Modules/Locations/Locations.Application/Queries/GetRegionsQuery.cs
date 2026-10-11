using Locations.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Locations.Application.Queries;

public sealed record GetRegionsQuery(string CountryCode) : IRequest<Result<IReadOnlyList<RegionDto>>>;
