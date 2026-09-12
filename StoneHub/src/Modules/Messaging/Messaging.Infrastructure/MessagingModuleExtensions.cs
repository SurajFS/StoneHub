using FluentValidation;
using MediatR;
using Messaging.Application;
using Messaging.Domain;
using Messaging.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Infrastructure;

namespace Messaging.Infrastructure;

public static class MessagingModuleExtensions
{
    public static IServiceCollection AddMessagingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventDispatch();
        services.AddDbContext<MessagingDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("StoneHub"),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "messaging"))
                .AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IMessagingQueryService, MessagingQueryService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IMessagingQueryService).Assembly));
        services.AddValidatorsFromAssembly(typeof(IMessagingQueryService).Assembly);

        return services;
    }
}
