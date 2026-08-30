using FluentValidation;
using Media.Application;
using Media.Domain;
using Media.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Infrastructure;

namespace Media.Infrastructure;

public static class MediaModuleExtensions
{
    public static IServiceCollection AddMediaModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventDispatch();
        services.AddDbContext<MediaDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("StoneUp"),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "media"))
                .AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.Configure<R2Settings>(configuration.GetSection(R2Settings.SectionName));

        services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
        services.AddSingleton<IStorageService, R2StorageService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IStorageService).Assembly));
        services.AddValidatorsFromAssembly(typeof(IStorageService).Assembly);

        return services;
    }
}
