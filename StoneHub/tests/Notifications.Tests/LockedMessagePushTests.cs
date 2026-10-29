using Billing.Application;
using Messaging.Domain;
using Notifications.Application;
using Notifications.Application.EventHandlers;
using Notifications.Domain;
using Xunit;

namespace Notifications.Tests;

// A Free seller/wholesaler must subscribe to read a message, so the push they get mustn't carry its
// text; everyone else still gets the preview.
public sealed class LockedMessagePushTests
{
    private const string Preview = "Is the 18mm slab still available?";

    [Fact]
    public async Task FreeOwnerRecipient_GetsPushWithoutMessageText()
    {
        var recipient = Guid.NewGuid();
        var sender = new RecordingPushSender();

        await Handle(recipient, sender, canUseOwnerFeatures: false);

        var push = Assert.Single(sender.Sent);
        Assert.DoesNotContain(Preview, push.Body);
        Assert.Equal("message", push.Data!["type"]);
    }

    [Fact]
    public async Task PayingRecipient_GetsPreview()
    {
        var recipient = Guid.NewGuid();
        var sender = new RecordingPushSender();

        await Handle(recipient, sender, canUseOwnerFeatures: true);

        Assert.Equal(Preview, Assert.Single(sender.Sent).Body);
    }

    private static Task Handle(Guid recipient, RecordingPushSender sender, bool canUseOwnerFeatures) =>
        new NewMessageReceivedEventHandler(new OneDevice(recipient), sender, new FixedEntitlements(canUseOwnerFeatures))
            .Handle(new NewMessageReceivedEvent(Guid.NewGuid(), recipient, Guid.NewGuid(), "Buyer name", Preview), CancellationToken.None);

    private sealed class RecordingPushSender : IPushSender
    {
        public List<PushMessage> Sent { get; } = [];

        public Task SendAsync(IReadOnlyList<string> expoPushTokens, PushMessage message, CancellationToken ct = default)
        {
            Sent.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedEntitlements(bool allowed) : IPlanEntitlements
    {
        public Task<bool> CanUseOwnerFeaturesAsync(Guid userId, CancellationToken ct = default) => Task.FromResult(allowed);
    }

    private sealed class OneDevice(Guid userId) : IDeviceTokenRepository
    {
        public Task<DeviceToken?> GetByTokenAsync(string expoPushToken, CancellationToken ct = default) =>
            Task.FromResult<DeviceToken?>(null);

        public Task<IReadOnlyList<DeviceToken>> GetByUserIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<DeviceToken>>(
                id == userId ? [DeviceToken.Create(userId, "ExponentPushToken[test]", DevicePlatform.Android)] : []);

        public void Add(DeviceToken token) { }

        public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
    }
}
