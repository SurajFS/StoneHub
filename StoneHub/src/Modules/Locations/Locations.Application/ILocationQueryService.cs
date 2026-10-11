using Locations.Application.Dtos;

namespace Locations.Application;

public interface ILocationQueryService
{
    Task<IReadOnlyList<CountryDto>> GetCountriesAsync(CancellationToken ct = default);

    Task<CountryDto?> GetCountryAsync(string countryCode, CancellationToken ct = default);

    Task<bool> RegionExistsAsync(string regionCode, CancellationToken ct = default);

    Task<IReadOnlyList<RegionDto>> GetRegionsAsync(string countryCode, CancellationToken ct = default);

    Task<IReadOnlyList<DistrictDto>> GetDistrictsAsync(string regionCode, CancellationToken ct = default);

    Task<IReadOnlyList<PostalLocalityDto>> GetPostalLocalitiesAsync(
        string countryCode, string postalCode, CancellationToken ct = default);
}
