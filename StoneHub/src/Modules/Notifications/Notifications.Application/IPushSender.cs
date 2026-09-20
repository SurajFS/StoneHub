namespace Notifications.Application;

public sealed record PushMessage(string Title, string Body, IReadOnlyDictionary<string, string>? Data = null);

// Implemented in Infrastructure against Expo's push HTTP API — Application code never knows
// it's Expo specifically, just "send this to these device tokens."
public interface IPushSender
{
    Task SendAsync(IReadOnlyList<string> expoPushTokens, PushMessage message, CancellationToken ct = default);
}
