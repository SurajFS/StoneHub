using Identity.Application.Profile;
using Xunit;

namespace Identity.Tests;

public sealed class AddressIssueRulesTests
{
    public static TheoryData<string, bool, bool, bool, AddressIssue[]> Cases => new()
    {
        // role, hasAddress, hasStreet, hasPostalCode, expected issues
        { "Seller", false, false, false, [AddressIssue.NoStructuredAddress] },
        { "Seller", true, false, false, [AddressIssue.MissingStreet, AddressIssue.MissingPostalCode] },
        { "Seller", true, true, false, [AddressIssue.MissingPostalCode] },
        { "Wholesaler", true, false, true, [AddressIssue.MissingStreet] },
        { "Wholesaler", true, true, true, [] },
        { "Buyer", false, false, false, [AddressIssue.NoStructuredAddress] },
        { "Buyer", true, false, false, [] },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void For_ReportsIssuesPerRole(string role, bool hasAddress, bool hasStreet, bool hasPostalCode, AddressIssue[] expected) =>
        Assert.Equal(expected, AddressIssueRules.For(role, hasAddress, hasStreet, hasPostalCode));
}
