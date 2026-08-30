using Amazon.S3;
using Amazon.S3.Model;
using Media.Application;
using Microsoft.Extensions.Options;

namespace Media.Infrastructure;

// Issues presigned PUT URLs against Cloudflare R2 (S3-compatible). Presigning is local
// crypto — no network call — so it works without a live bucket; the URL only resolves once
// R2 is provisioned. If R2 isn't configured, IsConfigured is false and callers short-circuit.
public sealed class R2StorageService : IStorageService
{
    private readonly R2Settings _settings;
    private readonly IAmazonS3? _client;

    public R2StorageService(IOptions<R2Settings> settings)
    {
        _settings = settings.Value;
        if (IsConfigured)
        {
            var config = new AmazonS3Config
            {
                ServiceURL = _settings.Endpoint,
                ForcePathStyle = true,
                AuthenticationRegion = "auto", // R2 signs with region "auto"
            };
            _client = new AmazonS3Client(_settings.AccessKeyId, _settings.SecretAccessKey, config);
        }
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_settings.Endpoint) &&
        !string.IsNullOrWhiteSpace(_settings.AccessKeyId) &&
        !string.IsNullOrWhiteSpace(_settings.SecretAccessKey) &&
        !string.IsNullOrWhiteSpace(_settings.Bucket);

    public async Task<string> GeneratePresignedUploadUrlAsync(string key, string contentType, CancellationToken ct = default)
    {
        if (_client is null)
            throw new InvalidOperationException("R2 storage is not configured.");

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _settings.Bucket,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(_settings.UrlExpiryMinutes),
            ContentType = contentType,
        };

        return await _client.GetPreSignedURLAsync(request);
    }

    public string GetPublicUrl(string key)
    {
        var baseUrl = string.IsNullOrWhiteSpace(_settings.PublicBaseUrl) ? _settings.Endpoint : _settings.PublicBaseUrl;
        return $"{baseUrl.TrimEnd('/')}/{key}";
    }
}
