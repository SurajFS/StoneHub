namespace Identity.Application.Profile;

// Which address issues a profile has. Mirrors AddressRequirement: owners need street + PIN,
// buyers only a structured address. ProfileAddressIssueQueryService filters with the same rule
// in SQL; keep the two in step (AddressIssueRulesTests pins this side).
public static class AddressIssueRules
{
    public const string SellerRole = "Seller";
    public const string WholesalerRole = "Wholesaler";
    public const string BuyerRole = "Buyer";

    public static bool RequiresStreetAndPostalCode(string role) => role is SellerRole or WholesalerRole;

    public static IReadOnlyList<AddressIssue> For(string role, bool hasAddress, bool hasStreet, bool hasPostalCode)
    {
        if (!hasAddress)
            return [AddressIssue.NoStructuredAddress];
        if (!RequiresStreetAndPostalCode(role))
            return [];

        var issues = new List<AddressIssue>(2);
        if (!hasStreet)
            issues.Add(AddressIssue.MissingStreet);
        if (!hasPostalCode)
            issues.Add(AddressIssue.MissingPostalCode);
        return issues;
    }
}
