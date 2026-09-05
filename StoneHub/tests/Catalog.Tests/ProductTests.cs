using Catalog.Domain;
using SharedKernel;
using Xunit;

namespace Catalog.Tests;

public sealed class ProductTests
{
    private static readonly Guid Seller = Guid.NewGuid();
    private static readonly Guid Category = Guid.NewGuid();

    private static Result<Product> CreateValid(
        decimal? wholesalePrice = null,
        decimal? moq = null,
        IEnumerable<string>? tags = null) =>
        Product.CreateListing(
            Seller, ListingOwnerType.Seller, "Carrara Marble Slab", Category, subcategoryId: null,
            size: "320 x 160 cm", thickness: "18 mm", finish: "Polished", color: "White",
            unit: ProductUnit.Slab, tags: tags ?? [], quantityAvailable: 100,
            price: Money.Create(185m), wholesalePrice: wholesalePrice, minimumOrderQuantity: moq);

    [Fact]
    public void CreateListing_WithValidInput_SucceedsAndIsActive()
    {
        var result = CreateValid();

        Assert.True(result.IsSuccess);
        var product = result.Value;
        Assert.Equal(Category, product.CategoryId);
        Assert.True(product.IsActive);
        Assert.True(product.IsAvailable);
        Assert.Equal(ListingOwnerType.Seller, product.OwnerType);
    }

    [Fact]
    public void CreateListing_RaisesProductListedEvent()
    {
        var product = CreateValid().Value;

        Assert.Contains(product.DomainEvents, e => e is ProductListedEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateListing_BlankTitle_Fails(string title)
    {
        var result = Product.CreateListing(
            Seller, ListingOwnerType.Seller, title, Category, null, null, null, null, null,
            ProductUnit.Piece, [], 1, Money.Create(10m), null, null);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void CreateListing_EmptyCategory_Fails()
    {
        var result = Product.CreateListing(
            Seller, ListingOwnerType.Seller, "Idol", Guid.Empty, null, null, null, null, null,
            ProductUnit.Piece, [], 1, Money.Create(10m), null, null);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void CreateListing_NegativeQuantity_Fails()
    {
        var result = Product.CreateListing(
            Seller, ListingOwnerType.Seller, "Idol", Category, null, null, null, null, null,
            ProductUnit.Piece, [], -1, Money.Create(10m), null, null);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void CreateListing_WholesalePriceWithoutMoq_Fails() =>
        Assert.True(CreateValid(wholesalePrice: 150m, moq: null).IsFailure);

    [Fact]
    public void CreateListing_MoqWithoutWholesalePrice_Fails() =>
        Assert.True(CreateValid(wholesalePrice: null, moq: 20m).IsFailure);

    [Fact]
    public void CreateListing_WholesalePriceZero_Fails() =>
        Assert.True(CreateValid(wholesalePrice: 0m, moq: 20m).IsFailure);

    [Fact]
    public void CreateListing_MoqBelowOne_Fails() =>
        Assert.True(CreateValid(wholesalePrice: 150m, moq: 0m).IsFailure);

    [Fact]
    public void CreateListing_ValidWholesale_Succeeds()
    {
        var result = CreateValid(wholesalePrice: 150m, moq: 20m);

        Assert.True(result.IsSuccess);
        Assert.Equal(150m, result.Value.WholesalePrice);
        Assert.Equal(20m, result.Value.MinimumOrderQuantity);
    }

    [Fact]
    public void CreateListing_NormalizesTags()
    {
        var product = CreateValid(tags: ["  Krishna ", "krishna", "IDOL", "", "  "]).Value;

        Assert.Equal(new[] { "krishna", "idol" }, product.Tags);
    }

    [Fact]
    public void UpdateStock_ToZero_MarksUnavailable()
    {
        var product = CreateValid().Value;

        product.UpdateStock(0);

        Assert.False(product.IsAvailable);
        Assert.Equal(0, product.QuantityAvailable);
    }

    [Fact]
    public void ActivateDeactivate_TogglesIsActive()
    {
        var product = CreateValid().Value;

        product.Deactivate();
        Assert.False(product.IsActive);

        product.Activate();
        Assert.True(product.IsActive);
    }
}
