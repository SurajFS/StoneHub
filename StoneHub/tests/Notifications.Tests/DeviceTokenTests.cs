using Notifications.Domain;
using Xunit;

namespace Notifications.Tests;

public sealed class DeviceTokenTests
{
    [Fact]
    public void Create_SetsAllFields()
    {
        var userId = Guid.NewGuid();

        var token = DeviceToken.Create(userId, "ExponentPushToken[abc123]", DevicePlatform.Ios);

        Assert.Equal(userId, token.UserId);
        Assert.Equal("ExponentPushToken[abc123]", token.ExpoPushToken);
        Assert.Equal(DevicePlatform.Ios, token.Platform);
    }

    [Fact]
    public void ReassignTo_ChangesOwnerAndPlatform_UpdatesTimestamp()
    {
        var token = DeviceToken.Create(Guid.NewGuid(), "ExponentPushToken[abc123]", DevicePlatform.Ios);
        var originalUpdatedAt = token.UpdatedAt;
        var newOwner = Guid.NewGuid();

        token.ReassignTo(newOwner, DevicePlatform.Android);

        Assert.Equal(newOwner, token.UserId);
        Assert.Equal(DevicePlatform.Android, token.Platform);
        Assert.True(token.UpdatedAt >= originalUpdatedAt);
    }
}
