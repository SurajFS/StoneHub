using Catalog.Application;
using Catalog.Domain;
using Catalog.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure;

public static class CatalogModuleExtensions
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("StoneUp"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog")));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductQueryService, ProductQueryService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IProductQueryService).Assembly));
        services.AddValidatorsFromAssembly(typeof(IProductQueryService).Assembly);

        return services;
    }
}
