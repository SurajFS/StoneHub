using Identity.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Identity.Application.Auth;

public sealed class LoginCommandHandler(
    IIdentityService identityService,
    IJwtTokenService jwtTokenService) : IRequestHandler<LoginCommand, Result<AuthResultDto>>
{
    public async Task<Result<AuthResultDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var credentialsResult = await identityService.ValidateCredentialsAsync(request.Email, request.Password, ct);
        if (credentialsResult.IsFailure)
            return Result.Failure<AuthResultDto>(credentialsResult);

        var credentials = credentialsResult.Value;
        var token = jwtTokenService.GenerateToken(credentials.UserId, credentials.Email, credentials.Role);
        return Result.Success(new AuthResultDto(credentials.UserId, credentials.Email, credentials.Role, token));
    }
}
