namespace Locations.Application;

// What a user picked: codes from the location endpoints, plus an optional PIN and locality.
public sealed record LocationSelection(
    string CountryCode,
    string RegionCode,
    string DistrictCode,
    string? PostalCode,
    string? Locality);
