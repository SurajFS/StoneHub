using Advertising.Domain;
using Billing.Application;
using Catalog.Application;
using MediatR;
using SharedKernel;

namespace Advertising.Application.Campaigns;

public sealed class StartCampaignCommandHandler(
    ICampaignRepository campaigns,
    IProductLookup productLookup,
    IBillingQueryService billing) : IRequestHandler<StartCampaignCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(StartCampaignCommand request, CancellationToken ct)
    {
        // Backend-validated, not trusted from the client: Premium status and product ownership
        // are both re-checked here regardless of what the caller's app UI already gated on.
        var subscription = await billing.GetByUserIdAsync(request.SellerId, ct);
        if (subscription is null || subscription.Plan != "Premium" || subscription.Status != "Active")
            return Result.Forbidden<Guid>("Advertising is a Premium feature — upgrade to run campaigns.");

        var product = await productLookup.GetOwnerInfoAsync(request.ProductId, ct);
        if (product is null || !product.IsActive)
            return Result.NotFound<Guid>("Product not found.");
        if (product.OwnerId != request.SellerId)
            return Result.Forbidden<Guid>("You can only advertise your own listings.");

        var existing = await campaigns.GetActiveByProductIdAsync(request.ProductId, ct);
        if (existing is not null)
            return Result.Conflict<Guid>("This product already has an active campaign.");

        var campaign = Campaign.Start(request.SellerId, request.ProductId);
        campaigns.Add(campaign);
        await campaigns.SaveChangesAsync(ct);

        return Result.Success(campaign.Id);
    }
}
