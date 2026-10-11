using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Locations.Infrastructure.Import;

// The India Post directory shipped inside this assembly (gzipped CSV; see Data/README.md).
internal static class BundledPostalDirectory
{
    public const string DatasetName = "india-post-pincodes";
    private const string ResourceName = "Locations.IndiaPostPincodes";
    private const int VersionLength = 16;

    // Bump when parsing/normalisation rules change, so already-imported databases reload too.
    private const int ParserRevision = 3;

    public static async Task<byte[]> ReadCompressedAsync(CancellationToken ct)
    {
        await using var stream = typeof(BundledPostalDirectory).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' is missing.");
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, ct);
        return buffer.ToArray();
    }

    // Content hash of the bundled file plus the parser revision: changing either triggers a re-import.
    public static string VersionOf(byte[] compressed) =>
        $"{Convert.ToHexStringLower(SHA256.HashData(compressed))[..VersionLength]}-p{ParserRevision}";

    public static async Task<PostalDirectory> ParseAsync(byte[] compressed, CancellationToken ct)
    {
        using var gzip = new GZipStream(new MemoryStream(compressed), CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8);
        return await PostalDirectoryParser.ParseAsync(reader, ct);
    }
}
