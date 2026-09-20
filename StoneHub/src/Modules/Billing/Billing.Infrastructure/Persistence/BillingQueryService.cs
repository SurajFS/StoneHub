using Billing.Application;
using Billing.Application.Dtos;
using Billing.Domain;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Billing.Infrastructure.Persistence;

public sealed class BillingQueryService(BillingDbContext dbContext) : IBillingQueryService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    public async Task<SubscriptionDto?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await dbContext.Subscriptions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(ProjectToDto())
            .FirstOrDefaultAsync(ct);

    public async Task<PagedResult<SubscriptionDto>> SearchAsync(int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize <= 0 ? DefaultPageSize : pageSize, 1, MaxPageSize);

        var query = dbContext.Subscriptions.AsNoTracking().OrderByDescending(x => x.CreatedAt);
        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ProjectToDto())
            .ToListAsync(ct);

        return new PagedResult<SubscriptionDto>(items, total, page, pageSize);
    }

    private static System.Linq.Expressions.Expression<Func<Subscription, SubscriptionDto>> ProjectToDto() => x =>
        new SubscriptionDto(x.Id, x.UserId, x.Plan.ToString(), x.Status.ToString(), x.StartDate, x.ExpiryDate, x.PaymentNotes, x.CreatedAt);
}
