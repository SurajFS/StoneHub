using Identity.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Identity.Application.Auth;

public sealed class LoginCommandHandler(
    IIdentityService identityService,
    IAuthTokenIssuer tokenIssuer) : IRequestHandler<LoginCommand, Result<AuthResultDto>>
{
    public async Task<Result<AuthResultDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var credentialsResult = await identityService.ValidateCredentialsAsync(request.Email, request.Password, ct);
        if (credentialsResult.IsFailure)
            return Result.Failure<AuthResultDto>(credentialsResult);

        var credentials = credentialsResult.Value;
        var result = await tokenIssuer.IssueAsync(credentials.UserId, credentials.Email, credentials.Role, ct);
        return Result.Success(result);
    }
}
