namespace Locations.Application;

// A selection checked against the reference data, with canonical codes and display names.
public sealed record ResolvedLocation(
    string CountryCode,
    string RegionCode,
    string RegionName,
    string DistrictCode,
    string DistrictName,
    string? PostalCode,
    string? Locality);
