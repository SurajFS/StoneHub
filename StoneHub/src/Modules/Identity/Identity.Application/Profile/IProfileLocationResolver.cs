using SharedKernel;

namespace Identity.Application.Profile;

public interface IProfileLocationResolver
{
    // Structured path: validates the address against the Locations reference data and the role's
    // requirement. Legacy path (no Address, from installed APKs): takes the free-text city/state,
    // and keeps the stored Address only if the text still matches it.
    Task<Result<ResolvedProfileLocation>> ResolveAsync(ProfileLocationRequest request, CancellationToken ct = default);
}
