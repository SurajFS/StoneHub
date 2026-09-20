using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Application;
using Notifications.Domain;
using Notifications.Infrastructure.Persistence;
using SharedKernel.Infrastructure;

namespace Notifications.Infrastructure;

public static class NotificationsModuleExtensions
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventDispatch();
        services.AddDbContext<NotificationsDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("StoneHub"),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "notifications"))
                .AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>();
        services.AddHttpClient<IPushSender, ExpoPushSender>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IPushSender).Assembly));
        services.AddValidatorsFromAssembly(typeof(IPushSender).Assembly);

        return services;
    }
}
