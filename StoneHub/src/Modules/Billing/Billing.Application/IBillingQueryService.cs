using Billing.Application.Dtos;
using SharedKernel;

namespace Billing.Application;

public interface IBillingQueryService
{
    Task<SubscriptionDto?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<PagedResult<SubscriptionDto>> SearchAsync(int page, int pageSize, CancellationToken ct = default);
}
