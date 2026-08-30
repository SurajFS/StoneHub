using Media.Infrastructure;
using Microsoft.Extensions.Options;
using Xunit;

namespace Media.Tests;

public sealed class R2StorageServiceTests
{
    private static R2StorageService Service(R2Settings settings) => new(Options.Create(settings));

    private static R2Settings Configured() => new()
    {
        AccessKeyId = "test-access-key",
        SecretAccessKey = "test-secret-key",
        Endpoint = "https://acc123.r2.cloudflarestorage.com",
        Bucket = "stoneup-media",
        PublicBaseUrl = "https://cdn.stoneup.test",
        UrlExpiryMinutes = 15,
    };

    [Fact]
    public void IsConfigured_FalseWhenEmpty()
    {
        Assert.False(Service(new R2Settings()).IsConfigured);
    }

    [Fact]
    public void IsConfigured_TrueWhenAllSet()
    {
        Assert.True(Service(Configured()).IsConfigured);
    }

    [Fact]
    public async Task GeneratePresignedUploadUrl_ContainsBucketAndKey()
    {
        var service = Service(Configured());

        var url = await service.GeneratePresignedUploadUrlAsync("uploads/abc/file.jpg", "image/jpeg");

        Assert.StartsWith("https://", url);
        Assert.Contains("stoneup-media", url);
        Assert.Contains("uploads", url);
    }

    [Fact]
    public void GetPublicUrl_UsesPublicBaseUrl()
    {
        var service = Service(Configured());

        Assert.Equal("https://cdn.stoneup.test/uploads/abc/file.jpg", service.GetPublicUrl("uploads/abc/file.jpg"));
    }
}
