using SharedKernel;

namespace Notifications.Domain;

// One row per installed app instance that has registered for push. A user can have several
// (phone + tablet, or a reinstall leaving a stale token) — keyed by the Expo push token itself,
// since that's what uniquely identifies the installation, not the user.
public sealed class DeviceToken : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string ExpoPushToken { get; private set; } = string.Empty;
    public DevicePlatform Platform { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private DeviceToken() { }

    private DeviceToken(Guid id, Guid userId, string expoPushToken, DevicePlatform platform) : base(id)
    {
        UserId = userId;
        ExpoPushToken = expoPushToken;
        Platform = platform;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static DeviceToken Create(Guid userId, string expoPushToken, DevicePlatform platform) =>
        new(Guid.NewGuid(), userId, expoPushToken, platform);

    // A token can outlive a logout/login-as-someone-else on the same device (shared device,
    // or the same person re-registering) — reassign ownership rather than erroring.
    public void ReassignTo(Guid userId, DevicePlatform platform)
    {
        UserId = userId;
        Platform = platform;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
