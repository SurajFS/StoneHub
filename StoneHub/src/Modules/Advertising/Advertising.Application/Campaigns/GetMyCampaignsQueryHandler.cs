using Advertising.Application.Dtos;
using Catalog.Application;
using MediatR;
using SharedKernel;

namespace Advertising.Application.Campaigns;

public sealed class GetMyCampaignsQueryHandler(ICampaignQueryService queryService, IProductLookup productLookup)
    : IRequestHandler<GetMyCampaignsQuery, Result<IReadOnlyList<MyCampaignRowDto>>>
{
    public async Task<Result<IReadOnlyList<MyCampaignRowDto>>> Handle(GetMyCampaignsQuery request, CancellationToken ct)
    {
        var campaigns = await queryService.GetBySellerIdAsync(request.SellerId, ct);

        var rows = new List<MyCampaignRowDto>(campaigns.Count);
        foreach (var campaign in campaigns)
        {
            var product = await productLookup.GetOwnerInfoAsync(campaign.ProductId, ct);
            rows.Add(new MyCampaignRowDto(
                campaign.Id, campaign.ProductId, product?.Title ?? "(listing removed)",
                campaign.Status, campaign.StartedAt, campaign.StoppedAt));
        }

        return Result.Success<IReadOnlyList<MyCampaignRowDto>>(rows);
    }
}
