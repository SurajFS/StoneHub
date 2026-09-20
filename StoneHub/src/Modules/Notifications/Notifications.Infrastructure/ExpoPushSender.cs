using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Notifications.Application;

namespace Notifications.Infrastructure;

public sealed class ExpoPushSender(HttpClient httpClient, ILogger<ExpoPushSender> logger) : IPushSender
{
    private const string SendEndpoint = "https://exp.host/--/api/v2/push/send";
    // Expo's documented limit per request.
    private const int BatchSize = 100;

    public async Task SendAsync(IReadOnlyList<string> expoPushTokens, PushMessage message, CancellationToken ct = default)
    {
        for (var offset = 0; offset < expoPushTokens.Count; offset += BatchSize)
        {
            var batch = expoPushTokens.Skip(offset).Take(BatchSize)
                .Select(token => new ExpoPushRequest(token, message.Title, message.Body, message.Data))
                .ToList();

            try
            {
                var response = await httpClient.PostAsJsonAsync(SendEndpoint, batch, ct);
                if (!response.IsSuccessStatusCode)
                    logger.LogWarning(
                        "Expo push send failed with status {StatusCode} for {TokenCount} tokens",
                        response.StatusCode, batch.Count);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Push delivery is a fire-and-forget side effect of sending a message — it must
                // never fail the message-send itself (see NewMessageReceivedEventHandler).
                logger.LogError(ex, "Expo push send threw for {TokenCount} tokens", batch.Count);
            }
        }
    }

    private sealed record ExpoPushRequest(
        [property: JsonPropertyName("to")] string To,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("body")] string Body,
        [property: JsonPropertyName("data")] IReadOnlyDictionary<string, string>? Data);
}
