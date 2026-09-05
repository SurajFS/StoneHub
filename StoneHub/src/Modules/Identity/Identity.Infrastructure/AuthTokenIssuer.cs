using Identity.Application;
using Identity.Application.Dtos;
using Identity.Domain;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure;

public sealed class AuthTokenIssuer(
    IJwtTokenService jwtTokenService,
    IRefreshTokenService refreshTokenService,
    IRefreshTokenRepository refreshTokens,
    IOptions<JwtSettings> jwtSettings) : IAuthTokenIssuer
{
    private readonly JwtSettings _settings = jwtSettings.Value;

    public async Task<AuthResultDto> IssueAsync(Guid userId, string email, string role, CancellationToken ct = default)
    {
        var accessToken = jwtTokenService.GenerateToken(userId, email, role);

        var rawRefreshToken = refreshTokenService.GenerateRawToken();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(_settings.RefreshTokenDays);
        refreshTokens.Add(RefreshToken.Issue(userId, refreshTokenService.Hash(rawRefreshToken), expiresAt));
        await refreshTokens.SaveChangesAsync(ct);

        return new AuthResultDto(userId, email, role, accessToken, rawRefreshToken);
    }
}
