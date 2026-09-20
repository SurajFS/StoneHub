using Billing.Application.Dtos;
using Identity.Application;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

public sealed class GetAllSubscriptionsQueryHandler(IBillingQueryService queryService, IUserDirectory userDirectory)
    : IRequestHandler<GetAllSubscriptionsQuery, Result<PagedResult<AdminSubscriptionRowDto>>>
{
    public async Task<Result<PagedResult<AdminSubscriptionRowDto>>> Handle(
        GetAllSubscriptionsQuery request, CancellationToken ct)
    {
        var page = await queryService.SearchAsync(request.Page, request.PageSize, ct);

        // Low-traffic admin-only listing (page size capped in the query service), so one
        // directory lookup per row is an acceptable tradeoff over adding a bulk-lookup API.
        var rows = new List<AdminSubscriptionRowDto>(page.Items.Count);
        foreach (var subscription in page.Items)
        {
            var user = await userDirectory.GetAsync(subscription.UserId, ct);
            rows.Add(new AdminSubscriptionRowDto(
                subscription.UserId,
                user?.Name ?? "(unknown)",
                user?.Role ?? "(unknown)",
                subscription.Plan,
                subscription.Status,
                subscription.StartDate,
                subscription.ExpiryDate,
                subscription.PaymentNotes));
        }

        return Result.Success(new PagedResult<AdminSubscriptionRowDto>(rows, page.Total, page.Page, page.PageSize));
    }
}
