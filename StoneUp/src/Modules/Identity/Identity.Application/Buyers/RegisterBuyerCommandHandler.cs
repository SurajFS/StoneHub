using Identity.Application.Dtos;
using Identity.Domain;
using MediatR;
using SharedKernel;

namespace Identity.Application.Buyers;

public sealed class RegisterBuyerCommandHandler(
    IIdentityService identityService,
    IBuyerProfileRepository buyerProfileRepository,
    IAuthTokenIssuer tokenIssuer) : IRequestHandler<RegisterBuyerCommand, Result<AuthResultDto>>
{
    private const string Role = "Buyer";

    public async Task<Result<AuthResultDto>> Handle(RegisterBuyerCommand request, CancellationToken ct)
    {
        var userResult = await identityService.CreateUserAsync(request.Email, request.Password, Role, ct);
        if (userResult.IsFailure)
            return Result.Failure<AuthResultDto>(userResult);

        var buyerProfile = BuyerProfile.Register(userResult.Value, request.DisplayName, request.BuyerType, request.City);

        buyerProfileRepository.Add(buyerProfile);
        await buyerProfileRepository.SaveChangesAsync(ct);

        var result = await tokenIssuer.IssueAsync(userResult.Value, request.Email, Role, ct);
        return Result.Success(result);
    }
}
