using Media.Domain;
using Xunit;

namespace Media.Tests;

public sealed class MediaAssetTests
{
    private static MediaAsset NewAsset(Guid? owner = null) =>
        MediaAsset.Create(Guid.NewGuid(), owner ?? Guid.NewGuid(), "uploads/x/f.jpg", "image/jpeg", "https://cdn/f.jpg");

    [Fact]
    public void Create_StartsPending_WithNoEvents()
    {
        var asset = NewAsset();

        Assert.Equal(MediaStatus.Pending, asset.Status);
        Assert.Empty(asset.DomainEvents);
    }

    [Fact]
    public void MarkUploaded_SetsUploaded_AndRaisesEvent()
    {
        var asset = NewAsset();

        asset.MarkUploaded();

        Assert.Equal(MediaStatus.Uploaded, asset.Status);
        var evt = Assert.Single(asset.DomainEvents);
        Assert.IsType<MediaUploadedEvent>(evt);
    }

    [Fact]
    public void MarkUploaded_IsIdempotent_NoSecondEvent()
    {
        var asset = NewAsset();
        asset.MarkUploaded();
        asset.ClearDomainEvents();

        asset.MarkUploaded();

        Assert.Equal(MediaStatus.Uploaded, asset.Status);
        Assert.Empty(asset.DomainEvents);
    }
}
