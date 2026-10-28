using System.Net.Http.Headers;
using System.Text;
using Billing.Application;
using Billing.Application.Payments;
using Billing.Domain;
using Billing.Infrastructure.Payments;
using Billing.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SharedKernel.Infrastructure;

namespace Billing.Infrastructure;

public static class BillingModuleExtensions
{
    // Razorpay's API answers in well under a second; a hung call shouldn't hold a request open.
    private static readonly TimeSpan GatewayTimeout = TimeSpan.FromSeconds(15);

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
        services.AddScoped<IProcessedPaymentEventRepository, ProcessedPaymentEventRepository>();
        services.AddScoped<IBillingQueryService, BillingQueryService>();

        services.Configure<RazorpaySettings>(configuration.GetSection(RazorpaySettings.SectionName));
        services.AddHttpClient<IPaymentGateway, RazorpayGateway>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<RazorpaySettings>>().Value;
            client.Timeout = GatewayTimeout;
            if (!settings.IsConfigured)
                return; // IsAvailable is false, so no call is ever made with this client.

            client.BaseAddress = new Uri(settings.ApiBaseUrl);
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{settings.KeyId}:{settings.KeySecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        });

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IBillingQueryService).Assembly));
        services.AddValidatorsFromAssembly(typeof(IBillingQueryService).Assembly);

        services.AddHostedService<SubscriptionExpirySweepService>();

        return services;
    }
}
