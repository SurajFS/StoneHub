using Billing.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

public sealed class GetMySubscriptionQueryHandler(IBillingQueryService queryService)
    : IRequestHandler<GetMySubscriptionQuery, Result<SubscriptionDto>>
{
    public async Task<Result<SubscriptionDto>> Handle(GetMySubscriptionQuery request, CancellationToken ct)
    {
        var subscription = await queryService.GetByUserIdAsync(request.UserId, ct);
        return subscription is null
            ? Result.NotFound<SubscriptionDto>("No subscription found for this account.")
            : Result.Success(subscription);
    }
}
