using Locations.Infrastructure.Import;
using Xunit;

namespace Locations.Tests;

public sealed class PlaceNameFormatterTests
{
    [Theory]
    [InlineData("AJMER", "Ajmer")]
    [InlineData("SAWAI  MADHOPUR", "Sawai Madhopur")]
    [InlineData("ANDAMAN AND NICOBAR ISLANDS", "Andaman and Nicobar Islands")]
    [InlineData("EAST GODAVARI(KAKINADA)", "East Godavari(Kakinada)")]
    [InlineData("Niwari", "Niwari")]
    [InlineData("S.A.S Nagar", "S.A.S Nagar")]
    [InlineData(" Tarn Taran ", "Tarn Taran")]
    public void ToDisplayName(string raw, string expected) =>
        Assert.Equal(expected, PlaceNameFormatter.ToDisplayName(raw));

    [Theory]
    [InlineData("Madanganj Kishangarh H.O", "Madanganj Kishangarh")]
    [InlineData("Kishangarh City S.O", "Kishangarh City")]
    [InlineData("Peddakotla B.O", "Peddakotla")]
    [InlineData("Ernakulam HO", "Ernakulam")]
    [InlineData("Dhan Mandi   Kishan Garh S.O", "Dhan Mandi Kishan Garh")]
    [InlineData("Some Place B.O.", "Some Place")]
    [InlineData("Jaipur G.P.O.", "Jaipur G.P.O.")]
    [InlineData("M.I.Road S.O", "M.I.Road")]
    [InlineData("SOHO", "Soho")]
    [InlineData("Purani Basti S.O (Jaipur)", "Purani Basti")]
    [InlineData("Piplod SO Surat", "Piplod")]
    [InlineData("Fatehpur SO BhagalpBO", "Fatehpur")]
    [InlineData("Garenga B.O B.O", "Garenga")]
    [InlineData("Tandi BO keylong", "Tandi")]
    [InlineData("Gandhi Ho Colony", "Gandhi Ho Colony")]
    [InlineData("KISHANGARH BO", "Kishangarh")]
    public void LocalityName_StripsOfficeTypeSuffix(string officeName, string expected) =>
        Assert.Equal(expected, PlaceNameFormatter.LocalityName(officeName));

    [Theory]
    [InlineData("IN-RJ", "Ajmer", "IN-RJ-AJMER")]
    [InlineData("IN-RJ", "Sawai Madhopur", "IN-RJ-SAWAI-MADHOPUR")]
    [InlineData("IN-PB", "S.A.S Nagar", "IN-PB-S-A-S-NAGAR")]
    [InlineData("IN-AP", "East Godavari(Kakinada)", "IN-AP-EAST-GODAVARI-KAKINADA")]
    public void DistrictCode_IsStableSlug(string region, string district, string expected) =>
        Assert.Equal(expected, PlaceNameFormatter.DistrictCode(region, district));

    [Fact]
    public void DistrictCode_IgnoresCasing() =>
        Assert.Equal(
            PlaceNameFormatter.DistrictCode("IN-RJ", "AJMER"),
            PlaceNameFormatter.DistrictCode("IN-RJ", "Ajmer"));

    [Theory]
    [InlineData("THE DADRA AND NAGAR HAVELI AND DAMAN AND DIU", "DADRA AND NAGAR HAVELI AND DAMAN AND DIU")]
    [InlineData("Jammu & Kashmir", "JAMMU AND KASHMIR")]
    [InlineData("  tamil   nadu ", "TAMIL NADU")]
    public void StateKey_NormalisesForMatching(string raw, string expected) =>
        Assert.Equal(expected, PlaceNameFormatter.StateKey(raw));
}
