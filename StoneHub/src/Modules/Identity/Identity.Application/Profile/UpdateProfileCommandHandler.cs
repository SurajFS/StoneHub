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
    IMediator mediator) : IRequestHandler<UpdateProfileCommand, Result<MeDto>>
{
    private const string SellerRole = "Seller";
    private const string WholesalerRole = "Wholesaler";

    public async Task<Result<MeDto>> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var userResult = await identityService.GetUserByIdAsync(request.UserId, ct);
        if (userResult.IsFailure)
            return Result.Failure<MeDto>(userResult);

        var user = userResult.Value;

        // PUT is a full replace: every field the client sends overwrites the stored value.
        // Name/City are guaranteed non-empty by the validator; State/Phone are required for
        // owners, so a missing value is rejected here (not silently kept, and not left to the
        // domain to throw). WhatsApp/AvatarUrl are optional and clear when sent empty.
        if (user.Role == SellerRole)
        {
            var seller = await sellerProfiles.GetByUserIdAsync(user.UserId, ct);
            if (seller is null)
                return Result.Failure<MeDto>("Seller profile not found.");
            if (string.IsNullOrWhiteSpace(request.State))
                return Result.Failure<MeDto>("State is required.");
            if (string.IsNullOrWhiteSpace(request.Phone))
                return Result.Failure<MeDto>("Phone is required.");

            var location = Location.Create(request.City, request.State);
            seller.UpdateProfile(request.Name, location, request.Phone, request.WhatsAppNumber, request.AvatarUrl);
            await sellerProfiles.SaveChangesAsync(ct);
        }
        else if (user.Role == WholesalerRole)
        {
            var wholesaler = await wholesalerProfiles.GetByUserIdAsync(user.UserId, ct);
            if (wholesaler is null)
                return Result.Failure<MeDto>("Wholesaler profile not found.");
            if (string.IsNullOrWhiteSpace(request.State))
                return Result.Failure<MeDto>("State is required.");
            if (string.IsNullOrWhiteSpace(request.Phone))
                return Result.Failure<MeDto>("Phone is required.");

            var location = Location.Create(request.City, request.State);
            wholesaler.UpdateProfile(request.Name, location, request.Phone, request.WhatsAppNumber, request.AvatarUrl);
            await wholesalerProfiles.SaveChangesAsync(ct);
        }
        else
        {
            var buyer = await buyerProfiles.GetByUserIdAsync(user.UserId, ct);
            if (buyer is null)
                return Result.Failure<MeDto>("Buyer profile not found.");

            buyer.UpdateProfile(request.Name, request.City, request.AvatarUrl);
            await buyerProfiles.SaveChangesAsync(ct);
        }

        // Return the refreshed view so the client updates in place without a second round-trip.
        return await mediator.Send(new GetMeQuery(request.UserId), ct);
    }
}
