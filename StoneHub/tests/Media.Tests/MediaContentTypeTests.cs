using Media.Application;
using Xunit;

namespace Media.Tests;

public sealed class MediaContentTypeTests
{
    [Theory]
    [InlineData("image/jpeg", true)]
    [InlineData("image/png", true)]
    [InlineData("video/mp4", true)]
    [InlineData("application/pdf", false)]
    [InlineData("", false)]
    public void IsAllowed_MatchesAllowList(string contentType, bool expected)
    {
        Assert.Equal(expected, MediaContentType.IsAllowed(contentType));
    }

    [Fact]
    public void ExtensionFor_KnownType_ReturnsExtension()
    {
        Assert.Equal(".jpg", MediaContentType.ExtensionFor("image/jpeg"));
    }

    [Fact]
    public void ExtensionFor_UnknownType_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, MediaContentType.ExtensionFor("application/pdf"));
    }
}
