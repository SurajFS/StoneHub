using Locations.Application.Dtos;
using MediatR;

namespace Locations.Application.Queries;

public sealed record GetCountriesQuery : IRequest<IReadOnlyList<CountryDto>>;
