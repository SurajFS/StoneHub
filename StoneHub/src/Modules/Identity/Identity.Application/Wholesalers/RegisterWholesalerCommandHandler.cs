using Identity.Application.Dtos;
using Identity.Domain;
using MediatR;
using SharedKernel;

namespace Identity.Application.Wholesalers;

public sealed class RegisterWholesalerCommandHandler(
    IIdentityService identityService,
    IWholesalerProfileRepository wholesalerProfileRepository,
    IAuthTokenIssuer tokenIssuer) : IRequestHandler<RegisterWholesalerCommand, Result<AuthResultDto>>
{
    private const string Role = "Wholesaler";

    public async Task<Result<AuthResultDto>> Handle(RegisterWholesalerCommand request, CancellationToken ct)
    {
        var userResult = await identityService.CreateUserAsync(request.Email, request.Password, Role, ct);
        if (userResult.IsFailure)
            return Result.Failure<AuthResultDto>(userResult);

        var location = Location.Create(request.City, request.State);
        var wholesalerProfile = WholesalerProfile.Register(
            userResult.Value, request.BusinessName, location, request.Phone, request.WhatsAppNumber);

        wholesalerProfileRepository.Add(wholesalerProfile);
        await wholesalerProfileRepository.SaveChangesAsync(ct);

        var result = await tokenIssuer.IssueAsync(userResult.Value, request.Email, Role, ct);
        return Result.Success(result);
    }
}
