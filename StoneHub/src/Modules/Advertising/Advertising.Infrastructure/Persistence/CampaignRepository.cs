using Advertising.Domain;
using Microsoft.EntityFrameworkCore;

namespace Advertising.Infrastructure.Persistence;

public sealed class CampaignRepository(AdvertisingDbContext dbContext) : ICampaignRepository
{
    public Task<Campaign?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.Campaigns.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Campaign?> GetActiveByProductIdAsync(Guid productId, CancellationToken ct = default) =>
        dbContext.Campaigns.FirstOrDefaultAsync(x => x.ProductId == productId && x.Status == CampaignStatus.Active, ct);

    public async Task<IReadOnlyList<Campaign>> GetBySellerIdAsync(Guid sellerId, CancellationToken ct = default) =>
        await dbContext.Campaigns
            .Where(x => x.SellerId == sellerId)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Campaign>> GetActiveBySellerIdAsync(Guid sellerId, CancellationToken ct = default) =>
        await dbContext.Campaigns
            .Where(x => x.SellerId == sellerId && x.Status == CampaignStatus.Active)
            .ToListAsync(ct);

    public void Add(Campaign campaign) => dbContext.Campaigns.Add(campaign);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
