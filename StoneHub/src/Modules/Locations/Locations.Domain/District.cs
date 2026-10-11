using SharedKernel;

namespace Locations.Domain;

// A district within a region, which is the "City" level of an Indian address. Id is a stable code
// derived from the region and name ("IN-RJ-AJMER") so it stays the same when the postal
// directory is re-imported. Loaded in bulk by the importer, never constructed in code.
public sealed class District : Entity<string>
{
    public string RegionId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    private District() { }
}
