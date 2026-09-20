using Identity.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Identity.Application.Admin;

public sealed class RegisterAdminCommandHandler(
    IIdentityService identityService,
    IAuthTokenIssuer tokenIssuer) : IRequestHandler<RegisterAdminCommand, Result<AuthResultDto>>
{
    private const string Role = "Admin";

    public async Task<Result<AuthResultDto>> Handle(RegisterAdminCommand request, CancellationToken ct)
    {
        var userResult = await identityService.CreateUserAsync(request.Email, request.Password, Role, ct);
        if (userResult.IsFailure)
            return Result.Failure<AuthResultDto>(userResult);

        var result = await tokenIssuer.IssueAsync(userResult.Value, request.Email, Role, ct);
        return Result.Success(result);
    }
}
