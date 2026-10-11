using Locations.Domain;
using Locations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Locations.Infrastructure.Import;

// Loads the bundled postal directory into the districts/localities tables when the bundled file
// differs from what was last imported. It replaces the country's rows in one transaction, so
// readers see either the old set or the new one. District codes are deterministic, so a
// re-import keeps every district a profile already references (unless the source drops it).
public sealed class PostalDirectoryImporter(LocationsDbContext dbContext, ILogger<PostalDirectoryImporter> logger)
{
    // Arbitrary app-wide key for pg_advisory_xact_lock; serialises imports across instances.
    private const long ImportLockKey = 7_412_306_001;

    public async Task ImportIfChangedAsync(CancellationToken ct)
    {
        var compressed = await BundledPostalDirectory.ReadCompressedAsync(ct);
        var version = BundledPostalDirectory.VersionOf(compressed);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
        await dbContext.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({ImportLockKey})", ct);

        var import = await dbContext.DatasetImports.FirstOrDefaultAsync(x => x.Id == BundledPostalDirectory.DatasetName, ct);
        if (import?.Version == version)
        {
            logger.LogDebug("Postal directory {Version} already imported", version);
            return;
        }

        var directory = await BundledPostalDirectory.ParseAsync(compressed, ct);
        await ReplaceCountryRowsAsync(directory, ct);

        if (import is null)
            dbContext.DatasetImports.Add(new DatasetImport(BundledPostalDirectory.DatasetName, version));
        else
            import.MarkImported(version);

        await dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        logger.LogInformation(
            "Postal directory {Version} imported: {DistrictCount} districts, {LocalityCount} localities",
            version, directory.Districts.Count, directory.Localities.Count);
    }

    private async Task ReplaceCountryRowsAsync(PostalDirectory directory, CancellationToken ct)
    {
        var countryCode = directory.CountryCode;
        var regionPrefix = countryCode + "-";

        await dbContext.PostalLocalities.Where(x => x.CountryId == countryCode).ExecuteDeleteAsync(ct);
        await dbContext.Districts.Where(x => x.RegionId.StartsWith(regionPrefix)).ExecuteDeleteAsync(ct);

        var connection = dbContext.Database.GetDbConnection() as NpgsqlConnection
            ?? throw new InvalidOperationException("Postal directory import requires a PostgreSQL connection.");

        await CopyDistrictsAsync(connection, directory.Districts, ct);
        await CopyLocalitiesAsync(connection, countryCode, directory.Localities, ct);
    }

    // Binary COPY joins the connection's open transaction; ~150k rows load in about a second.
    private static async Task CopyDistrictsAsync(
        NpgsqlConnection connection, IReadOnlyList<PostalDirectory.DistrictRow> rows, CancellationToken ct)
    {
        await using var writer = await connection.BeginBinaryImportAsync(
            $"COPY {LocationsDbContext.Schema}.\"{DistrictConfiguration.Table}\" (\"Id\", \"RegionId\", \"Name\") FROM STDIN (FORMAT BINARY)", ct);

        foreach (var row in rows)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(row.Code, NpgsqlDbType.Varchar, ct);
            await writer.WriteAsync(row.RegionCode, NpgsqlDbType.Varchar, ct);
            await writer.WriteAsync(row.Name, NpgsqlDbType.Varchar, ct);
        }

        await writer.CompleteAsync(ct);
    }

    private static async Task CopyLocalitiesAsync(
        NpgsqlConnection connection, string countryCode, IReadOnlyList<PostalDirectory.LocalityRow> rows, CancellationToken ct)
    {
        await using var writer = await connection.BeginBinaryImportAsync(
            $"COPY {LocationsDbContext.Schema}.\"{PostalLocalityConfiguration.Table}\" (\"CountryId\", \"PostalCode\", \"DistrictId\", \"Name\") FROM STDIN (FORMAT BINARY)", ct);

        foreach (var row in rows)
        {
            await writer.StartRowAsync(ct);
            await writer.WriteAsync(countryCode, NpgsqlDbType.Varchar, ct);
            await writer.WriteAsync(row.PostalCode, NpgsqlDbType.Varchar, ct);
            await writer.WriteAsync(row.DistrictCode, NpgsqlDbType.Varchar, ct);
            await writer.WriteAsync(row.Name, NpgsqlDbType.Varchar, ct);
        }

        await writer.CompleteAsync(ct);
    }
}
