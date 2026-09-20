using Billing.Domain;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

public sealed class ExpireDueSubscriptionsCommandHandler(ISubscriptionRepository subscriptions)
    : IRequestHandler<ExpireDueSubscriptionsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ExpireDueSubscriptionsCommand request, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var due = await subscriptions.GetDueForExpiryAsync(now, ct);

        var expiredCount = 0;
        foreach (var subscription in due)
        {
            if (subscription.ExpireIfDue(now))
                expiredCount++;
        }

        if (expiredCount > 0)
            await subscriptions.SaveChangesAsync(ct);

        return Result.Success(expiredCount);
    }
}
