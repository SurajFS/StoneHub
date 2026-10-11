using SharedKernel;

namespace Locations.Application;

// Published contract: lets other modules (Identity) validate and resolve addresses without
// reaching into the locations tables. Consumed via DI.
public interface ILocationDirectory
{
    // Checks that the region belongs to the country, the district to the region, the PIN (if
    // given) matches the country's format and serves that district, and the locality (if given)
    // is served by that PIN. Failures are field-level validation errors keyed by the selection's
    // property names (CountryCode, RegionCode, DistrictCode, PostalCode, Locality).
    Task<Result<ResolvedLocation>> ResolveAsync(LocationSelection selection, CancellationToken ct = default);

    // Exact, case-insensitive match of legacy free-text city/state onto a district. With no state,
    // matches only when the name is unique in the country. Returns null when there's no safe match.
    Task<ResolvedLocation?> MatchLegacyAsync(
        string countryCode, string city, string? state, CancellationToken ct = default);
}
