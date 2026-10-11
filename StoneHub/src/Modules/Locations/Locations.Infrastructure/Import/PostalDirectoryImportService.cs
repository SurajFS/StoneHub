using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Locations.Infrastructure.Import;

// Runs the postal-directory import once at startup, off the request path. Startup migrations
// in Program.cs finish before hosted services start, so the tables exist by the time this runs.
public sealed class PostalDirectoryImportService(
    IServiceScopeFactory scopeFactory,
    ILogger<PostalDirectoryImportService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var importer = scope.ServiceProvider.GetRequiredService<PostalDirectoryImporter>();
            await importer.ImportIfChangedAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Never crash the host over reference data. The import retries on the next start, and
            // until then the location endpoints keep serving the previously imported set.
            logger.LogError(ex, "Postal directory import failed");
        }
    }
}
