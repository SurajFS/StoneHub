using System.Security.Cryptography;
using System.Text;
using Identity.Application;

namespace Identity.Infrastructure;

public sealed class RefreshTokenService : IRefreshTokenService
{
    // 256 bits of entropy, base64-encoded — the raw value handed to the client.
    public string GenerateRawToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    // Only the hash is stored, so a DB leak doesn't expose usable tokens.
    public string Hash(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
