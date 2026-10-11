using Locations.Domain;
using Locations.Infrastructure.Import;
using Xunit;

namespace Locations.Tests;

// Guards the bundled dataset itself: a data refresh that introduces an unknown state, a bad PIN,
// or an over-long name fails here instead of at import time on a server.
public sealed class BundledPostalDirectoryTests
{
    private const string IndiaPattern = "^[1-9][0-9]{5}$";
    private const int RegionCount = 36;
    private const int DistrictCodeMaxLength = 100;
    private const int LocalityNameMaxLength = 150;

    private static readonly Lazy<Task<PostalDirectory>> Directory = new(async () =>
    {
        var compressed = await BundledPostalDirectory.ReadCompressedAsync(CancellationToken.None);
        return await BundledPostalDirectory.ParseAsync(compressed, CancellationToken.None);
    });

    [Fact]
    public async Task CoversEveryStateAndUnionTerritory()
    {
        var directory = await Directory.Value;

        Assert.Equal(RegionCount, directory.Districts.Select(x => x.RegionCode).Distinct().Count());
    }

    [Fact]
    public async Task EveryPostalCodeMatchesTheIndiaFormat()
    {
        var directory = await Directory.Value;

        Assert.All(directory.Localities, locality => Assert.True(PostalCodeRule.Matches(IndiaPattern, locality.PostalCode)));
    }

    [Fact]
    public async Task ValuesFitTheirColumns()
    {
        var directory = await Directory.Value;

        Assert.All(directory.Districts, district => Assert.InRange(district.Code.Length, 1, DistrictCodeMaxLength));
        Assert.All(directory.Localities, locality => Assert.InRange(locality.Name.Length, 1, LocalityNameMaxLength));
    }

    [Fact]
    public async Task EveryLocalityReferencesAParsedDistrict()
    {
        var directory = await Directory.Value;
        var districtCodes = directory.Districts.Select(x => x.Code).ToHashSet();

        Assert.All(directory.Localities, locality => Assert.Contains(locality.DistrictCode, districtCodes));
    }

    [Fact]
    public async Task ResolvesKishangarhPinToAjmerRajasthan()
    {
        var directory = await Directory.Value;

        var kishangarh = directory.Localities.Where(x => x.PostalCode == "305801").ToList();
        Assert.Contains(kishangarh, x => x.Name == "Madanganj Kishangarh");
        Assert.All(kishangarh, x => Assert.Equal("IN-RJ-AJMER", x.DistrictCode));
    }

    [Fact]
    public async Task ContainsNoArmyPostalOrTestLocalities()
    {
        var directory = await Directory.Value;

        Assert.DoesNotContain(directory.Localities, x => x.PostalCode.StartsWith('9'));
        Assert.DoesNotContain(directory.Localities, x => x.Name.Equals("Test Office", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task VersionIsAStableContentHash()
    {
        var compressed = await BundledPostalDirectory.ReadCompressedAsync(CancellationToken.None);

        Assert.Equal(BundledPostalDirectory.VersionOf(compressed), BundledPostalDirectory.VersionOf(compressed));
        Assert.Matches("^[0-9a-f]{16}-p[0-9]+$", BundledPostalDirectory.VersionOf(compressed));
    }
}
