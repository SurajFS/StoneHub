using Locations.Domain;
using Xunit;

namespace Locations.Tests;

public sealed class PostalCodeRuleTests
{
    private const string IndiaPattern = "^[1-9][0-9]{5}$";

    [Theory]
    [InlineData("305801", true)]
    [InlineData(" 302001 ", true)]
    [InlineData("110001", true)]
    [InlineData("012345", false)]
    [InlineData("30580", false)]
    [InlineData("3058011", false)]
    [InlineData("30580A", false)]
    [InlineData("305 801", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void Matches_IndiaPattern(string postalCode, bool expected) =>
        Assert.Equal(expected, PostalCodeRule.Matches(IndiaPattern, postalCode));

    [Theory]
    [InlineData("SW1A 1AA", true)]
    [InlineData("", false)]
    public void Matches_NoPattern_AcceptsAnyNonBlank(string postalCode, bool expected) =>
        Assert.Equal(expected, PostalCodeRule.Matches(null, postalCode));
}
