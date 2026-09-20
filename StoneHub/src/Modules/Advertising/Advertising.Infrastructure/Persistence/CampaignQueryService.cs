using Advertising.Application;
using Advertising.Application.Dtos;
using Advertising.Domain;
using Microsoft.EntityFrameworkCore;

namespace Advertising.Infrastructure.Persistence;

public sealed class CampaignQueryService(AdvertisingDbContext dbContext) : ICampaignQueryService
{
    public async Task<IReadOnlyList<CampaignDto>> GetBySellerIdAsync(Guid sellerId, CancellationToken ct = default) =>
        await dbContext.Campaigns
            .AsNoTracking()
            .Where(x => x.SellerId == sellerId)
            .OrderByDescending(x => x.StartedAt)
            .Select(x => new CampaignDto(x.Id, x.ProductId, x.Status.ToString(), x.StartedAt, x.StoppedAt))
            .ToListAsync(ct);
}
