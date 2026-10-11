namespace Locations.Infrastructure.Persistence;

// Countries available at launch. India only for now (decision 2026-10-11). The schema already
// supports more: add a row here (and its regions in RegionSeed) in a new migration.
internal static class CountrySeed
{
    public const string India = "IN";
    public const string IndiaName = "India";

    // India Post PIN: six digits, first digit 1-9.
    public const string IndiaPostalCodePattern = "^[1-9][0-9]{5}$";
}
