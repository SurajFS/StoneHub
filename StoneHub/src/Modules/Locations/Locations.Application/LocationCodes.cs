namespace Locations.Application;

// Input-format rules for location codes, shared by the query validators. Codes are stored
// upper-case, so handlers Normalize() before querying.
public static class LocationCodes
{
    public const string CountryCodePattern = "^[A-Za-z]{2}$";
    public const string RegionCodePattern = "^[A-Za-z]{2}-[A-Za-z0-9]{1,3}$";
    public const int MaxPostalCodeLength = 12;

    public static string Normalize(string code) => code.Trim().ToUpperInvariant();
}
