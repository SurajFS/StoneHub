using System.Text;
using System.Text.RegularExpressions;

namespace Locations.Infrastructure.Import;

// Pure helpers that turn the postal directory's raw names into display names and stable codes.
internal static partial class PlaceNameFormatter
{
    private static readonly HashSet<string> LowercaseWords = new(StringComparer.Ordinal) { "and", "of" };

    // An India Post office-type tag and anything after it: "Kishangarh B.O", "Ajmer H.O",
    // "Purani Basti S.O (Jaipur)", "Piplod SO Surat". What follows the tag repeats the district
    // (sometimes truncated, as in "BhagalpBO"), and the district is returned alongside anyway.
    // Case-sensitive: the source writes tags upper-case, so a word like "Ho" in a name is kept.
    [GeneratedRegex(@"\s+(?:[BSH]\.?\s?O|P\.?\s?O)\.?(?:\s.*)?$")]
    private static partial Regex OfficeSuffix();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    public static string CollapseWhitespace(string value) => Whitespace().Replace(value.Trim(), " ");

    // Normalises a state name for matching: upper-case, '&' → AND, no leading "THE".
    public static string StateKey(string value)
    {
        var key = CollapseWhitespace(value.Replace("&", " and ")).ToUpperInvariant();
        return key.StartsWith("THE ", StringComparison.Ordinal) ? key["THE ".Length..] : key;
    }

    // The source mixes "AJMER" and "Niwari". Upper-case names are title-cased; mixed-case names
    // are trusted as written, because they carry intentional casing such as "S.A.S Nagar".
    public static string ToDisplayName(string raw)
    {
        var collapsed = CollapseWhitespace(raw);
        if (collapsed.Any(char.IsLower))
            return collapsed;

        var words = collapsed.ToLowerInvariant().Split(' ');
        return string.Join(' ', words.Select((word, index) =>
            index > 0 && LowercaseWords.Contains(word) ? word : CapitaliseWord(word)));
    }

    public static string LocalityName(string officeName) =>
        ToDisplayName(OfficeSuffix().Replace(CollapseWhitespace(officeName), string.Empty));

    // "IN-RJ" + "Sawai Madhopur" → "IN-RJ-SAWAI-MADHOPUR". Stable across re-imports and casing.
    public static string DistrictCode(string regionCode, string districtName)
    {
        var builder = new StringBuilder(regionCode.Length + districtName.Length + 1).Append(regionCode);
        var pendingSeparator = true;
        foreach (var ch in districtName.ToUpperInvariant())
        {
            if (char.IsAsciiLetterOrDigit(ch))
            {
                if (pendingSeparator)
                    builder.Append('-');
                builder.Append(ch);
                pendingSeparator = false;
            }
            else
            {
                pendingSeparator = true;
            }
        }

        return builder.ToString();
    }

    // Upper-cases the first letter and any letter after '.', '(' or '-': "s.a.s" → "S.A.S".
    private static string CapitaliseWord(string word)
    {
        var chars = word.ToCharArray();
        var capitaliseNext = true;
        for (var i = 0; i < chars.Length; i++)
        {
            if (capitaliseNext && char.IsLetter(chars[i]))
            {
                chars[i] = char.ToUpperInvariant(chars[i]);
                capitaliseNext = false;
            }
            else if (chars[i] is '.' or '(' or '-')
            {
                capitaliseNext = true;
            }
            else if (char.IsLetterOrDigit(chars[i]))
            {
                capitaliseNext = false;
            }
        }

        return new string(chars);
    }
}
