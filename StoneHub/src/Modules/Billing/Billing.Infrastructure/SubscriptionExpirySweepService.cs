using Billing.Application.Subscriptions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Billing.Infrastructure;

// Daily automatic-downgrade sweep for expired Premium subscriptions. A plain BackgroundService
// for now — Hangfire is the documented choice for scheduled jobs in this codebase but isn't
// wired into the project yet; move this there once it is, so it shares scheduling/retry/
// dashboard with the Requirement module's future quote-expiry sweep.
public sealed class SubscriptionExpirySweepService(
    IServiceScopeFactory scopeFactory,
    ILogger<SubscriptionExpirySweepService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            await RunSweepAsync(stoppingToken);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunSweepAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var result = await mediator.Send(new ExpireDueSubscriptionsCommand(), ct);

            if (result.IsSuccess && result.Value > 0)
                logger.LogInformation("Subscription expiry sweep downgraded {Count} accounts to Free", result.Value);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // A failed sweep retries on the next tick; never crash the host over one bad run.
            logger.LogError(ex, "Subscription expiry sweep failed");
        }
    }
}
