using Identity.Application.Auth;
using Identity.Application.Dtos;
using Identity.Domain;
using MediatR;
using SharedKernel;

namespace Identity.Application.Profile;

public sealed class UpdateProfileCommandHandler(
    IIdentityService identityService,
    ISellerProfileRepository sellerProfiles,
    IWholesalerProfileRepository wholesalerProfiles,
    IBuyerProfileRepository buyerProfiles,
    IProfileLocationResolver locationResolver,
    IMediator mediator) : IRequestHandler<UpdateProfileCommand, Result<MeDto>>
{
    private const string SellerRole = "Seller";
    private const string WholesalerRole = "Wholesaler";
    private const string BuyerRole = "Buyer";

    // PUT is a full replace: every field the client sends overwrites the stored value. Name is
    // guaranteed by the validator. Owners must end up with a state and a phone, so a missing
    // value is rejected here (not silently kept). WhatsApp/AvatarUrl clear when sent empty.
    public async Task<Result<MeDto>> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var userResult = await identityService.GetUserByIdAsync(request.UserId, ct);
        if (userResult.IsFailure)
            return Result.Failure<MeDto>(userResult);

        var role = userResult.Value.Role;
        var update = role switch
        {
            SellerRole => await UpdateSellerAsync(request, ct),
            WholesalerRole => await UpdateWholesalerAsync(request, ct),
            _ => await UpdateBuyerAsync(request, ct),
        };
        if (update.IsFailure)
            return Result.Failure<MeDto>(update);

        // Return the refreshed view so the client updates in place without a second round-trip.
        return await mediator.Send(new GetMeQuery(request.UserId), ct);
    }

    private async Task<Result> UpdateSellerAsync(UpdateProfileCommand request, CancellationToken ct)
    {
        var seller = await sellerProfiles.GetByUserIdAsync(request.UserId, ct);
        if (seller is null)
            return Result.Failure("Seller profile not found.");

        var location = await ResolveOwnerLocationAsync(request, SellerRole, seller.Location, seller.Address, ct);
        if (location.IsFailure)
            return location;
        if (string.IsNullOrWhiteSpace(request.Phone))
            return Result.Failure("Phone is required.");

        var (display, address) = location.Value;
        seller.UpdateProfile(request.Name, display, address, request.Phone, request.WhatsAppNumber, request.AvatarUrl);
        await sellerProfiles.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result> UpdateWholesalerAsync(UpdateProfileCommand request, CancellationToken ct)
    {
        var wholesaler = await wholesalerProfiles.GetByUserIdAsync(request.UserId, ct);
        if (wholesaler is null)
            return Result.Failure("Wholesaler profile not found.");

        var location = await ResolveOwnerLocationAsync(request, WholesalerRole, wholesaler.Location, wholesaler.Address, ct);
        if (location.IsFailure)
            return location;
        if (string.IsNullOrWhiteSpace(request.Phone))
            return Result.Failure("Phone is required.");

        var (display, address) = location.Value;
        wholesaler.UpdateProfile(request.Name, display, address, request.Phone, request.WhatsAppNumber, request.AvatarUrl);
        await wholesalerProfiles.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result> UpdateBuyerAsync(UpdateProfileCommand request, CancellationToken ct)
    {
        var buyer = await buyerProfiles.GetByUserIdAsync(request.UserId, ct);
        if (buyer is null)
            return Result.Failure("Buyer profile not found.");

        var location = await locationResolver.ResolveAsync(
            new ProfileLocationRequest(BuyerRole, AddressRequirement.DistrictOnly, request.Address,
                request.City, request.State, buyer.City, buyer.State, buyer.Address),
            ct);
        if (location.IsFailure)
            return location;

        var resolved = location.Value;
        buyer.UpdateProfile(request.Name, resolved.City, resolved.State, resolved.Address, request.AvatarUrl);
        await buyerProfiles.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result<(Location Display, Address? Address)>> ResolveOwnerLocationAsync(
        UpdateProfileCommand request, string role, Location current, Address? currentAddress, CancellationToken ct)
    {
        var result = await locationResolver.ResolveAsync(
            new ProfileLocationRequest(role, AddressRequirement.StreetAndPostalCode, request.Address,
                request.City, request.State, current.City, current.State, currentAddress),
            ct);
        if (result.IsFailure)
            return Result.Failure<(Location, Address?)>(result);
        if (result.Value.State is null)
            return Result.Failure<(Location, Address?)>("State is required.");

        return Result.Success((Location.Create(result.Value.City, result.Value.State), result.Value.Address));
    }
}
