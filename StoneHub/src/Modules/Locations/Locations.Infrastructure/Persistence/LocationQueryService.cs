using Locations.Application;
using Locations.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Locations.Infrastructure.Persistence;

public sealed class LocationQueryService(LocationsDbContext dbContext) : ILocationQueryService
{
    public async Task<IReadOnlyList<CountryDto>> GetCountriesAsync(CancellationToken ct = default) =>
        await dbContext.Countries
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CountryDto(x.Id, x.Name, x.HasRegions, x.PostalCodePattern))
            .ToListAsync(ct);

    public async Task<CountryDto?> GetCountryAsync(string countryCode, CancellationToken ct = default) =>
        await dbContext.Countries
            .AsNoTracking()
            .Where(x => x.Id == countryCode)
            .Select(x => new CountryDto(x.Id, x.Name, x.HasRegions, x.PostalCodePattern))
            .FirstOrDefaultAsync(ct);

    public async Task<bool> RegionExistsAsync(string regionCode, CancellationToken ct = default) =>
        await dbContext.Regions.AsNoTracking().AnyAsync(x => x.Id == regionCode, ct);

    public async Task<IReadOnlyList<RegionDto>> GetRegionsAsync(string countryCode, CancellationToken ct = default) =>
        await dbContext.Regions
            .AsNoTracking()
            .Where(x => x.CountryId == countryCode)
            .OrderBy(x => x.Name)
            .Select(x => new RegionDto(x.Id, x.Name))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<DistrictDto>> GetDistrictsAsync(string regionCode, CancellationToken ct = default) =>
        await dbContext.Districts
            .AsNoTracking()
            .Where(x => x.RegionId == regionCode)
            .OrderBy(x => x.Name)
            .Select(x => new DistrictDto(x.Id, x.Name))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<PostalLocalityDto>> GetPostalLocalitiesAsync(
        string countryCode, string postalCode, CancellationToken ct = default) =>
        await (
            from locality in dbContext.PostalLocalities.AsNoTracking()
            join district in dbContext.Districts.AsNoTracking() on locality.DistrictId equals district.Id
            join region in dbContext.Regions.AsNoTracking() on district.RegionId equals region.Id
            where locality.CountryId == countryCode && locality.PostalCode == postalCode
            orderby locality.Name
            select new PostalLocalityDto(locality.Name, district.Id, district.Name, region.Id, region.Name))
            .ToListAsync(ct);
}
