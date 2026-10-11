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
        string? state;
        string? phone = null;
        string? whatsApp = null;
        string? avatarUrl;
        Address? address;

        if (user.Role == SellerRole)
        {
            var seller = await sellerProfiles.GetByUserIdAsync(user.UserId, ct);
            name = seller?.CompanyName;
            city = seller?.Location.City;
            state = seller?.Location.State;
            phone = seller?.Phone;
            whatsApp = seller?.WhatsAppNumber;
            avatarUrl = seller?.AvatarUrl;
            address = seller?.Address;
        }
        else if (user.Role == WholesalerRole)
        {
            var wholesaler = await wholesalerProfiles.GetByUserIdAsync(user.UserId, ct);
            name = wholesaler?.BusinessName;
            city = wholesaler?.Location.City;
            state = wholesaler?.Location.State;
            phone = wholesaler?.Phone;
            whatsApp = wholesaler?.WhatsAppNumber;
            avatarUrl = wholesaler?.AvatarUrl;
            address = wholesaler?.Address;
        }
        else
        {
            var buyer = await buyerProfiles.GetByUserIdAsync(user.UserId, ct);
            name = buyer?.DisplayName;
            city = buyer?.City;
            state = buyer?.State;
            avatarUrl = buyer?.AvatarUrl;
            address = buyer?.Address;
        }

        return Result.Success(new MeDto(
            user.UserId, user.Email, user.Role, name, city, state, phone, whatsApp, avatarUrl, ToDto(address)));
    }

    private static ProfileAddressDto? ToDto(Address? address) =>
        address is null
            ? null
            : new ProfileAddressDto(
                address.CountryCode, address.RegionCode, address.DistrictCode,
                address.Line1, address.Line2, address.PostalCode, address.Locality);
}
