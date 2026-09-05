using Catalog.Application;
using Catalog.Domain;
using Catalog.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Infrastructure;

namespace Catalog.Infrastructure;

public static class CatalogModuleExtensions
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventDispatch();
        services.AddDbContext<CatalogDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("StoneHub"),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog"))
                .AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductQueryService, ProductQueryService>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryQueryService, CategoryQueryService>();
        services.AddScoped<IProductLookup, ProductLookup>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IProductQueryService).Assembly));
        services.AddValidatorsFromAssembly(typeof(IProductQueryService).Assembly);

        return services;
    }
}
