namespace Locations.Application.Dtos;

public sealed record PostalCodeLookupDto(
    string CountryCode,
    string PostalCode,
    IReadOnlyList<PostalLocalityDto> Localities);
