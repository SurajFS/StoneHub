using Locations.Application;
using Locations.Domain;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Locations.Infrastructure.Persistence;

public sealed class LocationDirectory(LocationsDbContext dbContext) : ILocationDirectory
{
    public async Task<Result<ResolvedLocation>> ResolveAsync(LocationSelection selection, CancellationToken ct = default)
    {
        var countryCode = LocationCodes.Normalize(selection.CountryCode);
        var country = await dbContext.Countries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == countryCode, ct);
        if (country is null)
            return Invalid(nameof(LocationSelection.CountryCode), "Unknown country.");

        var regionCode = LocationCodes.Normalize(selection.RegionCode);
        var region = await dbContext.Regions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == regionCode && x.CountryId == countryCode, ct);
        if (region is null)
            return Invalid(nameof(LocationSelection.RegionCode), "Unknown state for this country.");

        var districtCode = LocationCodes.Normalize(selection.DistrictCode);
        var district = await dbContext.Districts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == districtCode && x.RegionId == regionCode, ct);
        if (district is null)
            return Invalid(nameof(LocationSelection.DistrictCode), "Unknown district for this state.");

        var postalCode = TrimToNull(selection.PostalCode);
        var locality = TrimToNull(selection.Locality);
        if (postalCode is null)
        {
            return locality is null
                ? Resolved(country, region, district, null, null)
                : Invalid(nameof(LocationSelection.PostalCode), "Enter the postal code for this locality.");
        }

        if (!country.IsValidPostalCode(postalCode))
            return Invalid(nameof(LocationSelection.PostalCode), "Postal code is not valid for this country.");

        var localities = await dbContext.PostalLocalities.AsNoTracking()
            .Where(x => x.CountryId == countryCode && x.PostalCode == postalCode && x.DistrictId == districtCode)
            .Select(x => x.Name)
            .ToListAsync(ct);
        if (localities.Count == 0)
            return Invalid(nameof(LocationSelection.PostalCode), "Postal code does not belong to this district.");

        if (locality is null)
            return Resolved(country, region, district, postalCode, null);

        var canonicalLocality = localities.FirstOrDefault(x => string.Equals(x, locality, StringComparison.OrdinalIgnoreCase));
        return canonicalLocality is null
            ? Invalid(nameof(LocationSelection.Locality), "Locality is not served by this postal code.")
            : Resolved(country, region, district, postalCode, canonicalLocality);
    }

    public async Task<ResolvedLocation?> MatchLegacyAsync(
        string countryCode, string city, string? state, CancellationToken ct = default)
    {
        var country = LocationCodes.Normalize(countryCode);
        var cityKey = city.Trim().ToLower();
        var stateKey = TrimToNull(state)?.ToLower();

        var candidates = await (
            from district in dbContext.Districts.AsNoTracking()
            join region in dbContext.Regions.AsNoTracking() on district.RegionId equals region.Id
            where region.CountryId == country
                  && district.Name.ToLower() == cityKey
                  && (stateKey == null || region.Name.ToLower() == stateKey)
            select new ResolvedLocation(country, region.Id, region.Name, district.Id, district.Name, null, null))
            .Take(2)
            .ToListAsync(ct);

        // Two hits means the same district name exists in more than one state, which only
        // happens when no state was given. Guessing would pin the profile to the wrong place.
        return candidates.Count == 1 ? candidates[0] : null;
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Result<ResolvedLocation> Resolved(
        Country country, Region region, District district, string? postalCode, string? locality) =>
        Result.Success(new ResolvedLocation(
            country.Id, region.Id, region.Name, district.Id, district.Name, postalCode, locality));

    private static Result<ResolvedLocation> Invalid(string field, string message) =>
        Result.ValidationFailure<ResolvedLocation>(new Dictionary<string, string[]> { [field] = [message] });
}
