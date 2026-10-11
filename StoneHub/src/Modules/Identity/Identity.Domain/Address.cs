using SharedKernel;

namespace Identity.Domain;

// A structured address chosen from the Locations reference data. Codes are canonical (ISO
// country + region, stable district code). Street lines, PIN and locality are optional at the
// domain level: backfilled addresses have only the codes. Which fields a role must supply is
// enforced where the address is entered (signup / profile edit), not here.
public sealed class Address : ValueObject
{
    public const int CodeMaxLength = 100;
    public const int LineMaxLength = 200;
    public const int PostalCodeMaxLength = 12;
    public const int LocalityMaxLength = 150;

    public string CountryCode { get; private set; } = string.Empty;
    public string RegionCode { get; private set; } = string.Empty;
    public string DistrictCode { get; private set; } = string.Empty;
    public string? Line1 { get; private set; }
    public string? Line2 { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Locality { get; private set; }

    private Address() { }

    public static Address Create(
        string countryCode,
        string regionCode,
        string districtCode,
        string? line1,
        string? line2,
        string? postalCode,
        string? locality)
    {
        if (string.IsNullOrWhiteSpace(countryCode)) throw new ArgumentException("Country is required.", nameof(countryCode));
        if (string.IsNullOrWhiteSpace(regionCode)) throw new ArgumentException("State is required.", nameof(regionCode));
        if (string.IsNullOrWhiteSpace(districtCode)) throw new ArgumentException("District is required.", nameof(districtCode));

        return new Address
        {
            CountryCode = countryCode.Trim(),
            RegionCode = regionCode.Trim(),
            DistrictCode = districtCode.Trim(),
            Line1 = TrimToNull(line1),
            Line2 = TrimToNull(line2),
            PostalCode = TrimToNull(postalCode),
            Locality = TrimToNull(locality),
        };
    }

    // What Sellers and Wholesalers must have for a complete address (future invoicing needs both).
    public bool HasStreetAndPostalCode => Line1 is not null && PostalCode is not null;

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return RegionCode;
        yield return DistrictCode;
        yield return Line1;
        yield return Line2;
        yield return PostalCode;
        yield return Locality;
    }
}
