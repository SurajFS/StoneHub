using Identity.Application.Dtos;
using Identity.Domain;
using MediatR;
using SharedKernel;

namespace Identity.Application.Sellers;

public sealed class RegisterSellerCommandHandler(
    IIdentityService identityService,
    ISellerProfileRepository sellerProfileRepository,
    IAuthTokenIssuer tokenIssuer) : IRequestHandler<RegisterSellerCommand, Result<AuthResultDto>>
{
    private const string Role = "Seller";

    public async Task<Result<AuthResultDto>> Handle(RegisterSellerCommand request, CancellationToken ct)
    {
        var userResult = await identityService.CreateUserAsync(request.Email, request.Password, Role, ct);
        if (userResult.IsFailure)
            return Result.Failure<AuthResultDto>(userResult);

        var location = Location.Create(request.City, request.State);
        var sellerProfile = SellerProfile.Register(
            userResult.Value, request.CompanyName, location, request.Phone, request.WhatsAppNumber);

        sellerProfileRepository.Add(sellerProfile);
        await sellerProfileRepository.SaveChangesAsync(ct);

        var result = await tokenIssuer.IssueAsync(userResult.Value, request.Email, Role, ct);
        return Result.Success(result);
    }
}
