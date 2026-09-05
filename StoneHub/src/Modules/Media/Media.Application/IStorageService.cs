namespace Media.Application;

// Abstraction over the blob store (Cloudflare R2). Presigning is a local signing operation,
// but exposed as async to stay compatible with the AWS SDK's async presign API.
public interface IStorageService
{
    bool IsConfigured { get; }
    Task<string> GeneratePresignedUploadUrlAsync(string key, string contentType, CancellationToken ct = default);
    string GetPublicUrl(string key);
}
