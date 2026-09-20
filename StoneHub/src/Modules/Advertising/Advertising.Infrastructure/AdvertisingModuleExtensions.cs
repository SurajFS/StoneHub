using Advertising.Application;
using Advertising.Domain;
using Advertising.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Infrastructure;

namespace Advertising.Infrastructure;

public static class AdvertisingModuleExtensions
{
    public static IServiceCollection AddAdvertisingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventDispatch();
        services.AddDbContext<AdvertisingDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("StoneHub"),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "advertising"))
                .AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<ICampaignQueryService, CampaignQueryService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ICampaignQueryService).Assembly));
        services.AddValidatorsFromAssembly(typeof(ICampaignQueryService).Assembly);

        return services;
    }
}
