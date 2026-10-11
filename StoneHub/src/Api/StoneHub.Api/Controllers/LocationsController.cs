using Locations.Application.Dtos;
using Locations.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoneHub.Api.Controllers;

// Public reference data for address entry: country → region (state) → district (city), plus
// postal-code lookup for autofill. The picker lists change only when a new dataset ships, so they
// carry a short (1h) public cache. Short so an empty list served before the first import finishes
// doesn't linger. Postal-code lookups are not cached, so a 400/404 is never pinned on a client.
[ApiController]
[Route("api/v1/locations")]
public sealed class LocationsController(IMediator mediator) : ControllerBase
{
    private const int ReferenceDataCacheSeconds = 60 * 60;

    [HttpGet("countries")]
    [ResponseCache(Duration = ReferenceDataCacheSeconds, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<IReadOnlyList<CountryDto>>> GetCountries(CancellationToken ct) =>
        Ok(await mediator.Send(new GetCountriesQuery(), ct));

    [HttpGet("countries/{countryCode}/regions")]
    [ResponseCache(Duration = ReferenceDataCacheSeconds, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType<IReadOnlyList<RegionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRegions(string countryCode, CancellationToken ct) =>
        (await mediator.Send(new GetRegionsQuery(countryCode), ct)).ToActionResult();

    [HttpGet("regions/{regionCode}/districts")]
    [ResponseCache(Duration = ReferenceDataCacheSeconds, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType<IReadOnlyList<DistrictDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDistricts(string regionCode, CancellationToken ct) =>
        (await mediator.Send(new GetDistrictsQuery(regionCode), ct)).ToActionResult();

    [HttpGet("countries/{countryCode}/postal-codes/{postalCode}")]
    [ProducesResponseType<PostalCodeLookupDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> LookupPostalCode(string countryCode, string postalCode, CancellationToken ct) =>
        (await mediator.Send(new LookupPostalCodeQuery(countryCode, postalCode), ct)).ToActionResult();
}
