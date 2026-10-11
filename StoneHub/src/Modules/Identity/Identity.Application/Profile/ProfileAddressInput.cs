namespace Identity.Application.Profile;

// A structured address as submitted at signup / profile edit. Codes come from the
// /api/v1/locations endpoints; Line1 is the street, Line2 the building / apartment.
public sealed record ProfileAddressInput(
    string CountryCode,
    string RegionCode,
    string DistrictCode,
    string? Line1,
    string? Line2,
    string? PostalCode,
    string? Locality);
