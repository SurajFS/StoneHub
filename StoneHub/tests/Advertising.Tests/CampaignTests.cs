using Advertising.Domain;
using Xunit;

namespace Advertising.Tests;

public sealed class CampaignTests
{
    [Fact]
    public void Start_SetsActiveWithNoStoppedAt_RaisesEvent()
    {
        var sellerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var campaign = Campaign.Start(sellerId, productId);

        Assert.Equal(sellerId, campaign.SellerId);
        Assert.Equal(productId, campaign.ProductId);
        Assert.Equal(CampaignStatus.Active, campaign.Status);
        Assert.Null(campaign.StoppedAt);
        var raised = Assert.Single(campaign.DomainEvents);
        var started = Assert.IsType<CampaignStartedEvent>(raised);
        Assert.Equal(productId, started.ProductId);
        Assert.Equal(sellerId, started.SellerId);
    }

    [Fact]
    public void Stop_WhenActive_SetsStoppedAndTimestamp_RaisesEvent()
    {
        var campaign = Campaign.Start(Guid.NewGuid(), Guid.NewGuid());
        campaign.ClearDomainEvents();

        campaign.Stop();

        Assert.Equal(CampaignStatus.Stopped, campaign.Status);
        Assert.NotNull(campaign.StoppedAt);
        var raised = Assert.Single(campaign.DomainEvents);
        var stopped = Assert.IsType<CampaignStoppedEvent>(raised);
        Assert.Equal(campaign.ProductId, stopped.ProductId);
    }

    [Fact]
    public void Stop_WhenAlreadyStopped_IsNoOp_RaisesNothing()
    {
        var campaign = Campaign.Start(Guid.NewGuid(), Guid.NewGuid());
        campaign.Stop();
        var stoppedAt = campaign.StoppedAt;
        campaign.ClearDomainEvents();

        campaign.Stop();

        Assert.Equal(stoppedAt, campaign.StoppedAt);
        Assert.Empty(campaign.DomainEvents);
    }
}
