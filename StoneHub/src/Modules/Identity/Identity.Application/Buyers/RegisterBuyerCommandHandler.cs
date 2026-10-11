using Identity.Application.Dtos;
using Identity.Application.Profile;
using Identity.Domain;
using MediatR;
using SharedKernel;

namespace Identity.Application.Buyers;

public sealed class RegisterBuyerCommandHandler(
    IIdentityService identityService,
    IBuyerProfileRepository buyerProfileRepository,
    IProfileLocationResolver locationResolver,
    IAuthTokenIssuer tokenIssuer) : IRequestHandler<RegisterBuyerCommand, Result<AuthResultDto>>
{
    private const string Role = "Buyer";

    public async Task<Result<AuthResultDto>> Handle(RegisterBuyerCommand request, CancellationToken ct)
    {
        // Resolve the location before creating the account, so a rejected address can't leave
        // behind a user with no profile.
        var locationResult = await locationResolver.ResolveAsync(
            new ProfileLocationRequest(Role, AddressRequirement.DistrictOnly, request.Address,
                request.City, null, null, null, null),
            ct);
        if (locationResult.IsFailure)
            return Result.Failure<AuthResultDto>(locationResult);
        var resolved = locationResult.Value;

        var userResult = await identityService.CreateUserAsync(request.Email, request.Password, Role, ct);
        if (userResult.IsFailure)
            return Result.Failure<AuthResultDto>(userResult);

        var buyerProfile = BuyerProfile.Register(
            userResult.Value, request.DisplayName, request.BuyerType, resolved.City, resolved.State, resolved.Address);

        buyerProfileRepository.Add(buyerProfile);
        await buyerProfileRepository.SaveChangesAsync(ct);

        var result = await tokenIssuer.IssueAsync(userResult.Value, request.Email, Role, ct);
        return Result.Success(result);
    }
}
