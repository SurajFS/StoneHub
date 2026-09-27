using Catalog.Domain;
using Xunit;

namespace Catalog.Tests;

public sealed class ProductDeleteAndMediaTests
{
    [Fact]
    public void Delete_MarksDeletedAndDeactivates()
    {
        var product = ProductTestData.Listing(Guid.NewGuid());

        product.Delete();

        Assert.NotNull(product.DeletedAt);
        Assert.False(product.IsActive);
    }

    [Fact]
    public void Delete_Twice_KeepsOriginalTimestamp()
    {
        var product = ProductTestData.Listing(Guid.NewGuid());
        product.Delete();
        var firstDeletedAt = product.DeletedAt;

        product.Delete();

        Assert.Equal(firstDeletedAt, product.DeletedAt);
    }

    [Fact]
    public void ReplaceMedia_SwapsTheWholeSet()
    {
        var product = ProductTestData.Listing(Guid.NewGuid());
        product.AddMedia(ProductMedia.Create("https://cdn/old.jpg", MediaType.Photo));

        product.ReplaceMedia([
            ProductMedia.Create("https://cdn/new.jpg", MediaType.Photo),
            ProductMedia.Create("https://cdn/new.mp4", MediaType.Video),
        ]);

        Assert.Equal(["https://cdn/new.jpg", "https://cdn/new.mp4"], product.Media.Select(m => m.Url));
    }

    [Fact]
    public void ReplaceMedia_WithEmpty_ClearsMedia()
    {
        var product = ProductTestData.Listing(Guid.NewGuid());
        product.AddMedia(ProductMedia.Create("https://cdn/old.jpg", MediaType.Photo));

        product.ReplaceMedia([]);

        Assert.Empty(product.Media);
    }
}
