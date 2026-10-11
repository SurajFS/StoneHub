using Locations.Application;
using SharedKernel;

namespace Identity.Tests;

// Stands in for the Locations module: knows Ajmer (PIN 305801 → Madanganj Kishangarh) and
// rejects everything else with the field-level errors the real directory returns.
internal sealed class FakeLocationDirectory : ILocationDirectory
{
    public const string AjmerDistrict = "IN-RJ-AJMER";
    public const string AjmerPin = "305801";
    public const string Kishangarh = "Madanganj Kishangarh";

    public int ResolveCalls { get; private set; }

    public Task<Result<ResolvedLocation>> ResolveAsync(LocationSelection selection, CancellationToken ct = default)
    {
        ResolveCalls++;
        if (selection.DistrictCode != AjmerDistrict)
            return Task.FromResult(Invalid(nameof(LocationSelection.DistrictCode)));
        if (selection.PostalCode is not null && selection.PostalCode != AjmerPin)
            return Task.FromResult(Invalid(nameof(LocationSelection.PostalCode)));

        var locality = string.Equals(selection.Locality, Kishangarh, StringComparison.OrdinalIgnoreCase) ? Kishangarh : null;
        return Task.FromResult(Result.Success(new ResolvedLocation(
            "IN", "IN-RJ", "Rajasthan", AjmerDistrict, "Ajmer", selection.PostalCode, locality)));
    }

    public Task<ResolvedLocation?> MatchLegacyAsync(string countryCode, string city, string? state, CancellationToken ct = default) =>
        Task.FromResult<ResolvedLocation?>(null);

    private static Result<ResolvedLocation> Invalid(string field) =>
        Result.ValidationFailure<ResolvedLocation>(new Dictionary<string, string[]> { [field] = ["invalid"] });
}
