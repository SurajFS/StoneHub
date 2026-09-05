namespace Media.Application;

// Allow-list of upload types (the "type check" from the roadmap). Virus scanning is not
// included — no AV service is wired yet.
public static class MediaContentType
{
    public static readonly IReadOnlyDictionary<string, string> AllowedExtensions = new Dictionary<string, string>
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["video/mp4"] = ".mp4",
    };

    public static bool IsAllowed(string contentType) => AllowedExtensions.ContainsKey(contentType);

    public static string ExtensionFor(string contentType) =>
        AllowedExtensions.TryGetValue(contentType, out var ext) ? ext : string.Empty;
}
