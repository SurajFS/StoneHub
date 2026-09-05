using Identity.Application.Dtos;
using Identity.Domain;
using MediatR;
using SharedKernel;

namespace Identity.Application.Auth;

public sealed class GetMeQueryHandler(
    IIdentityService identityService,
    ISellerProfileRepository sellerProfiles,
    IWholesalerProfileRepository wholesalerProfiles,
    IBuyerProfileRepository buyerProfiles) : IRequestHandler<GetMeQuery, Result<MeDto>>
{
    private const string SellerRole = "Seller";
    private const string WholesalerRole = "Wholesaler";

    public async Task<Result<MeDto>> Handle(GetMeQuery request, CancellationToken ct)
    {
        var userResult = await identityService.GetUserByIdAsync(request.UserId, ct);
        if (userResult.IsFailure)
            return Result.Failure<MeDto>(userResult);

        var user = userResult.Value;
        string? name;
        string? city;

        if (user.Role == SellerRole)
        {
            var seller = await sellerProfiles.GetByUserIdAsync(user.UserId, ct);
            name = seller?.CompanyName;
            city = seller?.Location.City;
        }
        else if (user.Role == WholesalerRole)
        {
            var wholesaler = await wholesalerProfiles.GetByUserIdAsync(user.UserId, ct);
            name = wholesaler?.BusinessName;
            city = wholesaler?.Location.City;
        }
        else
        {
            var buyer = await buyerProfiles.GetByUserIdAsync(user.UserId, ct);
            name = buyer?.DisplayName;
            city = buyer?.City;
        }

        return Result.Success(new MeDto(user.UserId, user.Email, user.Role, name, city));
    }
}
