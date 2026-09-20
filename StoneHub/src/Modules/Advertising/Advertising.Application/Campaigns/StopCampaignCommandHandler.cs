using Advertising.Domain;
using MediatR;
using SharedKernel;

namespace Advertising.Application.Campaigns;

public sealed class StopCampaignCommandHandler(ICampaignRepository campaigns)
    : IRequestHandler<StopCampaignCommand, Result>
{
    public async Task<Result> Handle(StopCampaignCommand request, CancellationToken ct)
    {
        var campaign = await campaigns.GetByIdAsync(request.CampaignId, ct);
        if (campaign is null)
            return Result.NotFound("Campaign not found.");
        if (campaign.SellerId != request.SellerId)
            return Result.Forbidden("You can only stop your own campaigns.");

        campaign.Stop();
        await campaigns.SaveChangesAsync(ct);

        return Result.Success();
    }
}
