namespace Media.Infrastructure;

// Cloudflare R2 (S3-compatible) config. Secrets (keys) come from user-secrets/env, never
// appsettings.json. Endpoint: https://<accountid>.r2.cloudflarestorage.com.
public sealed class R2Settings
{
    public const string SectionName = "R2";

    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Bucket { get; set; } = string.Empty;

    // Public/CDN base URL the app reads uploaded files from (custom domain or r2.dev).
    public string PublicBaseUrl { get; set; } = string.Empty;
    public int UrlExpiryMinutes { get; set; } = 15;
}
