using FluentValidation;
using Locations.Application;
using Locations.Infrastructure.Import;
using Locations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Locations.Infrastructure;

public static class LocationsModuleExtensions
{
    public static IServiceCollection AddLocationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LocationsDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("StoneHub"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", LocationsDbContext.Schema)));

        services.AddScoped<ILocationQueryService, LocationQueryService>();
        services.AddScoped<PostalDirectoryImporter>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ILocationQueryService).Assembly));
        services.AddValidatorsFromAssembly(typeof(ILocationQueryService).Assembly);

        services.AddHostedService<PostalDirectoryImportService>();

        return services;
    }
}
