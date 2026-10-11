using Identity.Application.Profile;
using Identity.Domain;
using Locations.Application;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Identity.Infrastructure;

public sealed class ProfileLocationResolver(ILocationDirectory locations, ILogger<ProfileLocationResolver> logger)
    : IProfileLocationResolver
{
    // Field keys mirror the request shape ("Address.PostalCode"), matching FluentValidation's
    // nested property names so clients map server errors onto fields one way.
    private const string AddressField = nameof(ProfileLocationRequest.Address);
    private const string CityField = "City";

    public async Task<Result<ResolvedProfileLocation>> ResolveAsync(
        ProfileLocationRequest request, CancellationToken ct = default) =>
        request.Address is null
            ? ResolveLegacy(request)
            : await ResolveStructuredAsync(request.Address, request.Requirement, ct);

    private async Task<Result<ResolvedProfileLocation>> ResolveStructuredAsync(
        ProfileAddressInput address, AddressRequirement requirement, CancellationToken ct)
    {
        var missing = MissingRequiredFields(address, requirement);
        if (missing.Count > 0)
            return Result.ValidationFailure<ResolvedProfileLocation>(missing);

        var resolved = await locations.ResolveAsync(
            new LocationSelection(address.CountryCode, address.RegionCode, address.DistrictCode, address.PostalCode, address.Locality),
            ct);
        if (resolved.IsFailure)
        {
            return resolved.ErrorType == ErrorType.Validation
                ? Result.ValidationFailure<ResolvedProfileLocation>(UnderAddress(resolved.ValidationErrors))
                : Result.Failure<ResolvedProfileLocation>(resolved);
        }

        var location = resolved.Value;
        var structured = Address.Create(
            location.CountryCode, location.RegionCode, location.DistrictCode,
            address.Line1, address.Line2, location.PostalCode, location.Locality);

        // Display city is the locality when one was picked ("Madanganj Kishangarh"), else the
        // district (decision 2026-10-11), so well-known towns stay visible on listings.
        return Result.Success(new ResolvedProfileLocation(location.Locality ?? location.DistrictName, location.RegionName, structured));
    }

    private Result<ResolvedProfileLocation> ResolveLegacy(ProfileLocationRequest request)
    {
        // Tracks how many installed APKs still send free text, to know when this path can go.
        // Information, not Warning: until M2 ships this is the normal path for every client.
        logger.LogInformation("Legacy free-text location submitted for {Role}", request.Role);

        if (string.IsNullOrWhiteSpace(request.LegacyCity))
        {
            return Result.ValidationFailure<ResolvedProfileLocation>(
                new Dictionary<string, string[]> { [CityField] = ["City is required."] });
        }

        var city = request.LegacyCity.Trim();
        var state = string.IsNullOrWhiteSpace(request.LegacyState) ? null : request.LegacyState.Trim();

        // An old client resends the whole profile. Keep the structured address while its display
        // text is unchanged. Once the user edits the text, the address no longer describes it.
        var unchanged = request.CurrentCity is not null
            && string.Equals(request.CurrentCity, city, StringComparison.OrdinalIgnoreCase)
            && string.Equals(request.CurrentState, state, StringComparison.OrdinalIgnoreCase);

        return Result.Success(new ResolvedProfileLocation(city, state, unchanged ? request.CurrentAddress : null));
    }

    private static Dictionary<string, string[]> MissingRequiredFields(ProfileAddressInput address, AddressRequirement requirement)
    {
        var errors = new Dictionary<string, string[]>();
        if (requirement != AddressRequirement.StreetAndPostalCode)
            return errors;

        if (string.IsNullOrWhiteSpace(address.Line1))
            errors[$"{AddressField}.{nameof(ProfileAddressInput.Line1)}"] = ["Street address is required."];
        if (string.IsNullOrWhiteSpace(address.PostalCode))
            errors[$"{AddressField}.{nameof(ProfileAddressInput.PostalCode)}"] = ["Postal code is required."];
        return errors;
    }

    private static Dictionary<string, string[]> UnderAddress(IReadOnlyDictionary<string, string[]> errors) =>
        errors.ToDictionary(entry => $"{AddressField}.{entry.Key}", entry => entry.Value);
}
