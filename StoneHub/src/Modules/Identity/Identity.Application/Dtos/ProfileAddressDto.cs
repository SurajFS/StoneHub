namespace Identity.Application.Dtos;

// The structured address on /auth/me. Codes match the /api/v1/locations endpoints. The display
// names already appear as MeDto.City / MeDto.State.
public sealed record ProfileAddressDto(
    string CountryCode,
    string RegionCode,
    string DistrictCode,
    string? Line1,
    string? Line2,
    string? PostalCode,
    string? Locality);
