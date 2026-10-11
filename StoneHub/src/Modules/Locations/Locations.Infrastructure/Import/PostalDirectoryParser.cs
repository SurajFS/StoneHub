using Locations.Infrastructure.Persistence;

namespace Locations.Infrastructure.Import;

// Parses the India Post "All India Pincode Directory" CSV into districts and localities. Pure: no
// I/O beyond the reader it is given. Throws InvalidDataException on a malformed file or an
// unknown state, because the bundled file is fixed and tested, so either means a bad data update.
internal static class PostalDirectoryParser
{
    private const string OfficeNameColumn = "OfficeName";
    private const string PincodeColumn = "Pincode";
    private const string DistrictColumn = "District";
    private const string StateColumn = "StateName";
    private const char ArmyPostalServicePrefix = '9';
    private const string TestOfficeName = "TEST OFFICE";

    private static readonly IReadOnlyDictionary<string, string> RegionCodeByStateKey =
        RegionSeed.India.ToDictionary(region => PlaceNameFormatter.StateKey(region.Name), region => region.Code);

    public static async Task<PostalDirectory> ParseAsync(TextReader reader, CancellationToken ct)
    {
        var header = await reader.ReadLineAsync(ct) ?? throw new InvalidDataException("Postal directory is empty.");
        var columns = ColumnIndexes(CsvLine.Split(header.TrimStart('﻿')));

        var districts = new Dictionary<string, PostalDirectory.DistrictRow>(StringComparer.Ordinal);
        var localities = new Dictionary<(string, string, string), PostalDirectory.LocalityRow>();
        var lineNumber = 1;

        while (await reader.ReadLineAsync(ct) is { } line)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = CsvLine.Split(line);
            if (fields.Count <= columns.Max)
                throw new InvalidDataException($"Postal directory line {lineNumber} has {fields.Count} fields.");
            if (IsNonCivilian(fields[columns.Pincode], fields[columns.OfficeName]))
                continue;

            var regionCode = RegionCodeFor(fields[columns.State], lineNumber);
            var districtName = PlaceNameFormatter.ToDisplayName(fields[columns.District]);
            var districtCode = PlaceNameFormatter.DistrictCode(regionCode, districtName);
            districts.TryAdd(districtCode, new PostalDirectory.DistrictRow(districtCode, regionCode, districtName));

            var postalCode = fields[columns.Pincode].Trim();
            var localityName = PlaceNameFormatter.LocalityName(fields[columns.OfficeName]);
            if (localityName.Length == 0)
                continue;

            var key = (postalCode, districtCode, localityName.ToUpperInvariant());
            localities.TryAdd(key, new PostalDirectory.LocalityRow(postalCode, districtCode, localityName));
        }

        return new PostalDirectory(CountrySeed.India, [.. districts.Values], [.. localities.Values]);
    }

    // PINs starting with 9 belong to the Army Postal Service (field post offices, not civilian
    // addresses). The source also ships a placeholder "TEST OFFICE" row. Neither should be offered.
    private static bool IsNonCivilian(string pincode, string officeName) =>
        pincode.TrimStart().StartsWith(ArmyPostalServicePrefix)
        || string.Equals(officeName.Trim(), TestOfficeName, StringComparison.OrdinalIgnoreCase);

    private static string RegionCodeFor(string stateName, int lineNumber) =>
        RegionCodeByStateKey.TryGetValue(PlaceNameFormatter.StateKey(stateName), out var code)
            ? code
            : throw new InvalidDataException($"Postal directory line {lineNumber}: unknown state '{stateName}'.");

    private static Columns ColumnIndexes(IReadOnlyList<string> header)
    {
        int IndexOf(string name)
        {
            for (var i = 0; i < header.Count; i++)
            {
                if (string.Equals(header[i].Trim(), name, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            throw new InvalidDataException($"Postal directory is missing the '{name}' column.");
        }

        return new Columns(IndexOf(OfficeNameColumn), IndexOf(PincodeColumn), IndexOf(DistrictColumn), IndexOf(StateColumn));
    }

    private sealed record Columns(int OfficeName, int Pincode, int District, int State)
    {
        public int Max => Math.Max(Math.Max(OfficeName, Pincode), Math.Max(District, State));
    }
}
