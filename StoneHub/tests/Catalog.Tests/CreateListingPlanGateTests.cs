using Billing.Application;
using Catalog.Application.Products;
using Catalog.Domain;
using Identity.Application;
using SharedKernel;
using Xunit;

namespace Catalog.Tests;

// Adding listings is a paid-plan feature: a Free seller must be refused with 402 before anything is
// saved, and a paying seller must still get through.
public sealed class CreateListingPlanGateTests
{
    private static readonly Category TopLevel = Category.Create("Marble", parentId: null, displayOrder: 0).Value;

    [Fact]
    public async Task FreeSeller_GetsPaymentRequired_NothingSaved()
    {
        var products = new FakeProductRepository();

        var result = await Handler(products, canUseOwnerFeatures: false).Handle(Command(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ErrorType.PaymentRequired, result.ErrorType);
        Assert.Equal(0, products.SaveCount);
    }

    [Fact]
    public async Task PayingSeller_CreatesListing()
    {
        var products = new FakeProductRepository();

        var result = await Handler(products, canUseOwnerFeatures: true).Handle(Command(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, products.SaveCount);
    }

    private static CreateProductListingCommandHandler Handler(FakeProductRepository products, bool canUseOwnerFeatures) =>
        new(products, new FakeCategoryRepository(TopLevel), new NoSellerDirectory(), new FixedEntitlements(canUseOwnerFeatures));

    private static CreateProductListingCommand Command(Guid sellerId) =>
        new(sellerId, "Seller", "Makrana white", TopLevel.Id, null, null, null, null, null, "Slab", [],
            10m, 250m, "INR", null, null, [], []);

    private sealed class FixedEntitlements(bool allowed) : IPlanEntitlements
    {
        public Task<bool> CanUseOwnerFeaturesAsync(Guid userId, CancellationToken ct = default) => Task.FromResult(allowed);
    }

    private sealed class NoSellerDirectory : ISellerDirectory
    {
        public Task<SellerDirectoryEntry?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
            Task.FromResult<SellerDirectoryEntry?>(null);
    }
}
