using Identity.Application.Dtos;
using Identity.Domain;
using Locations.Application;
using MediatR;
using SharedKernel;

namespace Identity.Application.Profile;

public sealed class BackfillProfileAddressesCommandHandler(
    ISellerProfileRepository sellerProfiles,
    IWholesalerProfileRepository wholesalerProfiles,
    IBuyerProfileRepository buyerProfiles,
    ILocationDirectory locations) : IRequestHandler<BackfillProfileAddressesCommand, Result<AddressBackfillReportDto>>
{
    // Legacy profiles predate country selection, and StoneHub was India-only when they were created.
    private const string LegacyCountryCode = "IN";

    // Exact name matches only. Locality-name matching would be wrong: the only Rajasthan
    // locality named exactly "Kishangarh" is in Jalore, but Kishangarh sellers are in Ajmer.
    // Anything that doesn't match stays free text for the user to fix.
    public async Task<Result<AddressBackfillReportDto>> Handle(BackfillProfileAddressesCommand request, CancellationToken ct)
    {
        var matched = 0;
        var unmatched = 0;

        foreach (var seller in await sellerProfiles.GetWithoutAddressAsync(ct))
        {
            var match = await locations.MatchLegacyAsync(LegacyCountryCode, seller.Location.City, seller.Location.State, ct);
            if (match is null) { unmatched++; continue; }
            seller.AttachAddress(Location.Create(match.DistrictName, match.RegionName), ToAddress(match));
            matched++;
        }
        await sellerProfiles.SaveChangesAsync(ct);

        foreach (var wholesaler in await wholesalerProfiles.GetWithoutAddressAsync(ct))
        {
            var match = await locations.MatchLegacyAsync(LegacyCountryCode, wholesaler.Location.City, wholesaler.Location.State, ct);
            if (match is null) { unmatched++; continue; }
            wholesaler.AttachAddress(Location.Create(match.DistrictName, match.RegionName), ToAddress(match));
            matched++;
        }
        await wholesalerProfiles.SaveChangesAsync(ct);

        foreach (var buyer in await buyerProfiles.GetWithoutAddressAsync(ct))
        {
            var match = await locations.MatchLegacyAsync(LegacyCountryCode, buyer.City, buyer.State, ct);
            if (match is null) { unmatched++; continue; }
            buyer.AttachAddress(match.DistrictName, match.RegionName, ToAddress(match));
            matched++;
        }
        await buyerProfiles.SaveChangesAsync(ct);

        return Result.Success(new AddressBackfillReportDto(matched, unmatched));
    }

    private static Address ToAddress(ResolvedLocation match) =>
        Address.Create(match.CountryCode, match.RegionCode, match.DistrictCode, null, null, null, null);
}
