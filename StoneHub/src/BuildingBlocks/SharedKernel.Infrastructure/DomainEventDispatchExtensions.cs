using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SharedKernel.Infrastructure;

public static class DomainEventDispatchExtensions
{
    // Registers the interceptor once even if several modules call it. Each module adds it to
    // its own DbContext options via .AddInterceptors(sp.GetRequiredService<...>()).
    public static IServiceCollection AddDomainEventDispatch(this IServiceCollection services)
    {
        services.TryAddScoped<DomainEventDispatchInterceptor>();
        return services;
    }
}
