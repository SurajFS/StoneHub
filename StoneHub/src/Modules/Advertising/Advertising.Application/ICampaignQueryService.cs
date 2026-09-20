using Advertising.Application.Dtos;

namespace Advertising.Application;

public interface ICampaignQueryService
{
    Task<IReadOnlyList<CampaignDto>> GetBySellerIdAsync(Guid sellerId, CancellationToken ct = default);
}
