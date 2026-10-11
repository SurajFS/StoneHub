using SharedKernel;

namespace Locations.Domain;

// A state / union territory / province. Id is the ISO 3166-2 code ("IN-RJ"). Seeded reference data.
public sealed class Region : Entity<string>
{
    public string CountryId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    private Region() { }
}
