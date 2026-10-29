using Billing.Application;
using MediatR;
using Messaging.Domain;
using Notifications.Domain;

namespace Notifications.Application.EventHandlers;

// A Free seller/wholesaler recipient is told a message arrived, but not what it says: they subscribe
// to read it, so the push mustn't leak the text either.
public sealed class NewMessageReceivedEventHandler(IDeviceTokenRepository tokens, IPushSender pushSender, IPlanEntitlements entitlements)
    : INotificationHandler<NewMessageReceivedEvent>
{
    private const string LockedPreview = "Subscribe to Basic or Premium to read and reply.";

    public async Task Handle(NewMessageReceivedEvent notification, CancellationToken ct)
    {
        var devices = await tokens.GetByUserIdAsync(notification.RecipientId, ct);
        if (devices.Count == 0)
            return;

        var message = new PushMessage(
            Title: $"New message from {notification.SenderName}",
            Body: await entitlements.CanUseOwnerFeaturesAsync(notification.RecipientId, ct)
                ? notification.Preview
                : LockedPreview,
            Data: new Dictionary<string, string>
            {
                ["type"] = "message",
                ["conversationId"] = notification.ConversationId.ToString(),
                ["senderName"] = notification.SenderName,
            });

        await pushSender.SendAsync(devices.Select(d => d.ExpoPushToken).ToList(), message, ct);
    }
}
