using Identity.Application.Dtos;
using Identity.Domain;
using MediatR;
using SharedKernel;

namespace Identity.Application.Auth;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenService refreshTokenService,
    IRefreshTokenRepository refreshTokens,
    IIdentityService identityService,
    IAuthTokenIssuer tokenIssuer) : IRequestHandler<RefreshTokenCommand, Result<AuthResultDto>>
{
    public async Task<Result<AuthResultDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var hash = refreshTokenService.Hash(request.RefreshToken);
        var token = await refreshTokens.GetByHashAsync(hash, ct);

        var now = DateTimeOffset.UtcNow;
        if (token is null || !token.IsActive(now))
            return Result.Unauthorized<AuthResultDto>("Invalid or expired refresh token.");

        var userResult = await identityService.GetUserByIdAsync(token.UserId, ct);
        if (userResult.IsFailure)
            return Result.Unauthorized<AuthResultDto>("Invalid refresh token.");

        // Rotate: revoke the presented token; the issuer adds the new one and saves both.
        token.Revoke(now);
        var user = userResult.Value;
        var result = await tokenIssuer.IssueAsync(user.UserId, user.Email, user.Role, ct);
        return Result.Success(result);
    }
}
