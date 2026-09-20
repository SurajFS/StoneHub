using SharedKernel;

namespace Advertising.Domain;

// A Premium seller promoting one product. Free (included in Premium, no separate charge —
// see task.md), so there's no budget/bidding concept: a campaign is just "currently promoted
// or not," with enough history to satisfy "view the status of their campaigns."
public sealed class Campaign : AggregateRoot<Guid>
{
    public Guid SellerId { get; private set; }
    public Guid ProductId { get; private set; }
    public CampaignStatus Status { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? StoppedAt { get; private set; }

    private Campaign() { }

    private Campaign(Guid id, Guid sellerId, Guid productId) : base(id)
    {
        SellerId = sellerId;
        ProductId = productId;
        Status = CampaignStatus.Active;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public static Campaign Start(Guid sellerId, Guid productId)
    {
        var campaign = new Campaign(Guid.NewGuid(), sellerId, productId);
        campaign.Raise(new CampaignStartedEvent(productId, sellerId));
        return campaign;
    }

    public void Stop()
    {
        if (Status != CampaignStatus.Active)
            return;

        Status = CampaignStatus.Stopped;
        StoppedAt = DateTimeOffset.UtcNow;
        Raise(new CampaignStoppedEvent(ProductId));
    }
}
