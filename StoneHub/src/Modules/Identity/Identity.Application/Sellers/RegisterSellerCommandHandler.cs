using Identity.Application.Dtos;
using Identity.Application.Profile;
using Identity.Domain;
using MediatR;
using SharedKernel;

namespace Identity.Application.Sellers;

public sealed class RegisterSellerCommandHandler(
    IIdentityService identityService,
    ISellerProfileRepository sellerProfileRepository,
    IProfileLocationResolver locationResolver,
    IAuthTokenIssuer tokenIssuer) : IRequestHandler<RegisterSellerCommand, Result<AuthResultDto>>
{
    private const string Role = "Seller";

    public async Task<Result<AuthResultDto>> Handle(RegisterSellerCommand request, CancellationToken ct)
    {
        // Resolve the location before creating the account, so a rejected address can't leave
        // behind a user with no profile.
        var locationResult = await locationResolver.ResolveAsync(
            new ProfileLocationRequest(Role, AddressRequirement.StreetAndPostalCode, request.Address,
                request.City, request.State, null, null, null),
            ct);
        if (locationResult.IsFailure)
            return Result.Failure<AuthResultDto>(locationResult);
        var resolved = locationResult.Value;
        if (resolved.State is null)
            return Result.Failure<AuthResultDto>("State is required.");

        var userResult = await identityService.CreateUserAsync(request.Email, request.Password, Role, ct);
        if (userResult.IsFailure)
            return Result.Failure<AuthResultDto>(userResult);

        var sellerProfile = SellerProfile.Register(
            userResult.Value, request.CompanyName, Location.Create(resolved.City, resolved.State), resolved.Address,
            request.Phone, request.WhatsAppNumber);

        sellerProfileRepository.Add(sellerProfile);
        await sellerProfileRepository.SaveChangesAsync(ct);

        var result = await tokenIssuer.IssueAsync(userResult.Value, request.Email, Role, ct);
        return Result.Success(result);
    }
}
