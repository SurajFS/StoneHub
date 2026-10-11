namespace Locations.Infrastructure.Import;

// The parsed, de-duplicated postal directory, ready to bulk-load.
internal sealed record PostalDirectory(
    string CountryCode,
    IReadOnlyList<PostalDirectory.DistrictRow> Districts,
    IReadOnlyList<PostalDirectory.LocalityRow> Localities)
{
    public sealed record DistrictRow(string Code, string RegionCode, string Name);

    public sealed record LocalityRow(string PostalCode, string DistrictCode, string Name);
}
