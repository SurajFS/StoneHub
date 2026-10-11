using Locations.Infrastructure.Import;
using Xunit;

namespace Locations.Tests;

public sealed class PostalDirectoryParserTests
{
    private const string Header =
        "\"CircleName\",\"RegionName\",\"DivisionName\",\"OfficeName\",\"Pincode\",\"OfficeType\",\"Delivery\",\"District\",\"StateName\",\"Latitude\",\"Longitude\"";

    private static string Row(string office, string pin, string district, string state) =>
        $"\"C\",\"R\",\"D\",\"{office}\",\"{pin}\",\"BO\",\"Delivery\",\"{district}\",\"{state}\",\"0\",\"0\"";

    private static Task<PostalDirectory> ParseAsync(params string[] rows) =>
        PostalDirectoryParser.ParseAsync(new StringReader(string.Join('\n', [Header, .. rows])), CancellationToken.None);

    [Fact]
    public async Task Parse_MapsStateToIsoCodeAndBuildsDistrict()
    {
        var directory = await ParseAsync(Row("Madanganj Kishangarh H.O", "305801", "AJMER", "RAJASTHAN"));

        var district = Assert.Single(directory.Districts);
        Assert.Equal(new PostalDirectory.DistrictRow("IN-RJ-AJMER", "IN-RJ", "Ajmer"), district);
        var locality = Assert.Single(directory.Localities);
        Assert.Equal(new PostalDirectory.LocalityRow("305801", "IN-RJ-AJMER", "Madanganj Kishangarh"), locality);
        Assert.Equal("IN", directory.CountryCode);
    }

    [Fact]
    public async Task Parse_MergesDistrictCaseVariantsAndDuplicateLocalities()
    {
        var directory = await ParseAsync(
            Row("Thal B.O", "305801", "AJMER", "RAJASTHAN"),
            Row("Thal BO", "305801", "Ajmer", "RAJASTHAN"),
            Row("Dindwara B.O", "305801", "AJMER", "RAJASTHAN"));

        Assert.Single(directory.Districts);
        Assert.Equal(["Dindwara", "Thal"], directory.Localities.Select(x => x.Name).Order());
    }

    [Fact]
    public async Task Parse_SamePinInTwoDistricts_KeepsBoth()
    {
        var directory = await ParseAsync(
            Row("Alpha B.O", "123456", "AJMER", "RAJASTHAN"),
            Row("Beta B.O", "123456", "JAIPUR", "RAJASTHAN"));

        Assert.Equal(2, directory.Districts.Count);
        Assert.Equal(2, directory.Localities.Count);
    }

    [Fact]
    public async Task Parse_HandlesTheDadraPrefixedStateName()
    {
        var directory = await ParseAsync(Row("Silvassa S.O", "396230", "DADRA AND NAGAR HAVELI", "THE DADRA AND NAGAR HAVELI AND DAMAN AND DIU"));

        Assert.Equal("IN-DH", Assert.Single(directory.Districts).RegionCode);
    }

    [Theory]
    [InlineData("1CBPO", "900056")]
    [InlineData("TEST OFFICE", "999999")]
    [InlineData("Test Office", "123456")]
    public async Task Parse_SkipsArmyPostalAndTestRows(string office, string pin)
    {
        var directory = await ParseAsync(Row(office, pin, "AJMER", "RAJASTHAN"));

        Assert.Empty(directory.Localities);
        Assert.Empty(directory.Districts);
    }

    [Fact]
    public async Task Parse_UnknownState_Throws() =>
        await Assert.ThrowsAsync<InvalidDataException>(() => ParseAsync(Row("X B.O", "123456", "Y", "ATLANTIS")));

    [Fact]
    public async Task Parse_MissingColumn_Throws() =>
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            PostalDirectoryParser.ParseAsync(new StringReader("\"OfficeName\",\"Pincode\""), CancellationToken.None));
}
