using Billing.Domain;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

public sealed class CancelSubscriptionCommandHandler(ISubscriptionRepository subscriptions)
    : IRequestHandler<CancelSubscriptionCommand, Result>
{
    public async Task<Result> Handle(CancelSubscriptionCommand request, CancellationToken ct)
    {
        var subscription = await subscriptions.GetByUserIdAsync(request.UserId, ct);
        if (subscription is null)
            return Result.NotFound("No subscription found for this account.");

        subscription.Cancel();
        await subscriptions.SaveChangesAsync(ct);

        return Result.Success();
    }
}
