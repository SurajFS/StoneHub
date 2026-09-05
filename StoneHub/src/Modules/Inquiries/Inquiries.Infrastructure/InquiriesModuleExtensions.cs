using FluentValidation;
using Inquiries.Application;
using Inquiries.Domain;
using Inquiries.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Infrastructure;

namespace Inquiries.Infrastructure;

public static class InquiriesModuleExtensions
{
    public static IServiceCollection AddInquiriesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventDispatch();
        services.AddDbContext<InquiryDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("StoneHub"),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "inquiries"))
                .AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<IInquiryRepository, InquiryRepository>();
        services.AddScoped<IInquiryQueryService, InquiryQueryService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IInquiryQueryService).Assembly));
        services.AddValidatorsFromAssembly(typeof(IInquiryQueryService).Assembly);

        return services;
    }
}
