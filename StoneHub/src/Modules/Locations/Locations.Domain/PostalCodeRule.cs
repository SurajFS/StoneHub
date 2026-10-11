using System.Text.RegularExpressions;

namespace Locations.Domain;

public static class PostalCodeRule
{
    private static readonly TimeSpan MatchTimeout = TimeSpan.FromMilliseconds(100);

    // A country without a known pattern accepts any non-blank postal code.
    public static bool Matches(string? pattern, string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return false;

        return pattern is null || Regex.IsMatch(postalCode.Trim(), pattern, RegexOptions.None, MatchTimeout);
    }
}
