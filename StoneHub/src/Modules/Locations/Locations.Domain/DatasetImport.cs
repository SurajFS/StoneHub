using SharedKernel;

namespace Locations.Domain;

// Records which version of a bundled dataset (Id = dataset name) is loaded, so the importer
// only reloads when the bundled file changes.
public sealed class DatasetImport : Entity<string>
{
    public string Version { get; private set; } = string.Empty;
    public DateTimeOffset ImportedAt { get; private set; }

    private DatasetImport() { }

    public DatasetImport(string datasetName, string version) : base(datasetName) => MarkImported(version);

    public void MarkImported(string version)
    {
        Version = version;
        ImportedAt = DateTimeOffset.UtcNow;
    }
}
