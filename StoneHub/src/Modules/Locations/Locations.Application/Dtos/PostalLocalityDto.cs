namespace Locations.Application.Dtos;

// A locality served by a postal code, with its district and region resolved for autofill.
public sealed record PostalLocalityDto(
    string Name,
    string DistrictCode,
    string DistrictName,
    string RegionCode,
    string RegionName);
