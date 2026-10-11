using SharedKernel;

namespace Locations.Domain;

// A country users can place an address in. Id is the ISO 3166-1 alpha-2 code ("IN").
// HasRegions says whether addresses pick a state/region from a list; PostalCodePattern is the
// country's postal-code format (null where we don't validate one). Seeded reference data.
public sealed class Country : Entity<string>
{
    public string Name { get; private set; } = string.Empty;
    public bool HasRegions { get; private set; }
    public string? PostalCodePattern { get; private set; }

    private Country() { }

    public bool IsValidPostalCode(string postalCode) => PostalCodeRule.Matches(PostalCodePattern, postalCode);
}
