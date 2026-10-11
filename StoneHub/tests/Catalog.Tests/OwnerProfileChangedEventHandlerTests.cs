using Catalog.Application.EventHandlers;
using Identity.Domain;
using Xunit;

namespace Catalog.Tests;

public sealed class OwnerProfileChangedEventHandlerTests
{
    [Fact]
    public async Task Handle_RefreshesOnlyThatOwnersListings()
    {
        var owner = Guid.NewGuid();
        var mine = ProductTestData.Listing(owner);
        var other = ProductTestData.Listing(Guid.NewGuid());
        other.SetSellerInfo("Other Co", "Jaipur, Rajasthan");
        var repository = new FakeProductRepository(mine, other);

        await new OwnerProfileChangedEventHandler(repository).Handle(
            new OwnerProfileChangedEvent(owner, "Kishangarh Marbles", "Madanganj Kishangarh, Rajasthan"), CancellationToken.None);

        Assert.Equal("Kishangarh Marbles", mine.SellerName);
        Assert.Equal("Madanganj Kishangarh, Rajasthan", mine.SellerLocation);
        Assert.Equal("Jaipur, Rajasthan", other.SellerLocation);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task Handle_OwnerWithNoListings_DoesNotSave()
    {
        var repository = new FakeProductRepository();

        await new OwnerProfileChangedEventHandler(repository).Handle(
            new OwnerProfileChangedEvent(Guid.NewGuid(), "X", "Y, Z"), CancellationToken.None);

        Assert.Equal(0, repository.SaveCount);
    }
}
