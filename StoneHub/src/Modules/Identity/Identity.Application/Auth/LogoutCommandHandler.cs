using Identity.Domain;
using MediatR;
using SharedKernel;

namespace Identity.Application.Auth;

public sealed class LogoutCommandHandler(
    IRefreshTokenService refreshTokenService,
    IRefreshTokenRepository refreshTokens) : IRequestHandler<LogoutCommand, Result>
{
    // Idempotent: revoking an unknown/already-revoked token still succeeds.
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        var hash = refreshTokenService.Hash(request.RefreshToken);
        var token = await refreshTokens.GetByHashAsync(hash, ct);
        if (token is not null)
        {
            token.Revoke(DateTimeOffset.UtcNow);
            await refreshTokens.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
