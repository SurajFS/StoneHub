using MediatR;
using Messaging.Domain;
using Notifications.Domain;

namespace Notifications.Application.EventHandlers;

public sealed class NewMessageReceivedEventHandler(IDeviceTokenRepository tokens, IPushSender pushSender)
    : INotificationHandler<NewMessageReceivedEvent>
{
    public async Task Handle(NewMessageReceivedEvent notification, CancellationToken ct)
    {
        var devices = await tokens.GetByUserIdAsync(notification.RecipientId, ct);
        if (devices.Count == 0)
            return;

        var message = new PushMessage(
            Title: $"New message from {notification.SenderName}",
            Body: notification.Preview,
            Data: new Dictionary<string, string>
            {
                ["type"] = "message",
                ["conversationId"] = notification.ConversationId.ToString(),
                ["senderName"] = notification.SenderName,
            });

        await pushSender.SendAsync(devices.Select(d => d.ExpoPushToken).ToList(), message, ct);
    }
}
