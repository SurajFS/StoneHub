using Catalog.Application.Products;
using Catalog.Domain;
using SharedKernel;
using Xunit;

namespace Catalog.Tests;

public sealed class UpdateProductCommandHandlerTests
{
    private static readonly Guid Owner = Guid.NewGuid();
    private static readonly Category TopLevel = Category.Create("Marble", parentId: null, displayOrder: 0).Value;

    private static UpdateProductCommand Command(Guid productId, Guid caller, List<string>? photos, List<string>? videos) =>
        new(productId, caller, "Updated title", TopLevel.Id, null, null, null, null, null, "Slab", [],
            200m, "INR", null, null, 5m, photos, videos);

    private static async Task<(Result Result, Product Product)> Run(Guid caller, List<string>? photos, List<string>? videos)
    {
        var product = ProductTestData.Listing(Owner);
        product.AddMedia(ProductMedia.Create("https://cdn/original.jpg", MediaType.Photo));
        var handler = new UpdateProductCommandHandler(new FakeProductRepository(product), new FakeCategoryRepository(TopLevel));

        var result = await handler.Handle(Command(product.Id, caller, photos, videos), CancellationToken.None);
        return (result, product);
    }

    [Fact]
    public async Task NoMediaLists_KeepsExistingMedia()
    {
        var (result, product) = await Run(Owner, photos: null, videos: null);

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated title", product.Title);
        Assert.Equal(["https://cdn/original.jpg"], product.Media.Select(m => m.Url));
    }

    [Fact]
    public async Task MediaLists_ReplaceMediaWithCorrectTypes()
    {
        var (result, product) = await Run(Owner, photos: ["https://cdn/a.jpg"], videos: ["https://cdn/v.mp4"]);

        Assert.True(result.IsSuccess);
        Assert.Collection(
            product.Media,
            m => Assert.Equal(("https://cdn/a.jpg", MediaType.Photo), (m.Url, m.MediaType)),
            m => Assert.Equal(("https://cdn/v.mp4", MediaType.Video), (m.Url, m.MediaType)));
    }

    [Fact]
    public async Task PhotosOnly_RemovesVideo()
    {
        var (_, product) = await Run(Owner, photos: [], videos: null);

        Assert.Empty(product.Media);
    }

    [Fact]
    public async Task OtherSeller_IsForbidden_AndNothingChanges()
    {
        var (result, product) = await Run(Guid.NewGuid(), photos: [], videos: []);

        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
        Assert.Equal("Carrara Marble Slab", product.Title);
        Assert.Single(product.Media);
    }
}
