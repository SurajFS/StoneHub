using Billing.Application;
using Billing.Domain;
using Billing.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Infrastructure;

namespace Billing.Infrastructure;

public static class BillingModuleExtensions
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventDispatch();
        services.AddDbContext<BillingDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("StoneHub"),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "billing"))
                .AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IBillingQueryService, BillingQueryService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IBillingQueryService).Assembly));
        services.AddValidatorsFromAssembly(typeof(IBillingQueryService).Assembly);

        services.AddHostedService<SubscriptionExpirySweepService>();

        return services;
    }
}
