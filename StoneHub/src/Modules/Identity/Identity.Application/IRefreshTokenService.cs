namespace Identity.Application;

// Generates opaque refresh tokens and hashes them for storage (impl in Infrastructure).
public interface IRefreshTokenService
{
    string GenerateRawToken();
    string Hash(string rawToken);
}
