namespace Locations.Application.Dtos;

public sealed record CountryDto(string Code, string Name, bool HasRegions, string? PostalCodePattern);
