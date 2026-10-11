using Identity.Application.Profile;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Identity.Infrastructure.Persistence;

// The report's read side: one UNION ALL over the three profile tables (joined to Users for the
// email), filtered in SQL to profiles with an issue, then paged. The filters encode the same
// rule as AddressIssueRules: owners need street + PIN, buyers just a structured address.
public sealed class ProfileAddressIssueQueryService(IdentityModuleDbContext dbContext) : IProfileAddressIssueQueryService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    public async Task<PagedResult<ProfileAddressRow>> SearchAsync(
        string? role, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize <= 0 ? DefaultPageSize : pageSize, 1, MaxPageSize);

        var query = RowsFor(role)
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.UserId);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<ProfileAddressRow>(items, total, page, pageSize);
    }

    private IQueryable<ProfileAddressRow> RowsFor(string? role)
    {
        var parts = new List<IQueryable<ProfileAddressRow>>(3);
        if (role is null or AddressIssueRules.SellerRole)
            parts.Add(Sellers());
        if (role is null or AddressIssueRules.WholesalerRole)
            parts.Add(Wholesalers());
        if (role is null or AddressIssueRules.BuyerRole)
            parts.Add(Buyers());

        return parts.Aggregate((combined, next) => combined.Concat(next));
    }

    private IQueryable<ProfileAddressRow> Sellers() =>
        from profile in dbContext.SellerProfiles.AsNoTracking()
        join user in dbContext.Users.AsNoTracking() on profile.UserId equals user.Id
        where profile.Address == null || profile.Address.Line1 == null || profile.Address.PostalCode == null
        select new ProfileAddressRow
        {
            UserId = profile.UserId,
            Email = user.Email ?? string.Empty,
            Role = AddressIssueRules.SellerRole,
            Name = profile.CompanyName,
            City = profile.Location.City,
            State = profile.Location.State,
            HasAddress = profile.Address != null,
            HasStreet = profile.Address != null && profile.Address.Line1 != null,
            HasPostalCode = profile.Address != null && profile.Address.PostalCode != null,
            CreatedAt = profile.CreatedAt,
        };

    private IQueryable<ProfileAddressRow> Wholesalers() =>
        from profile in dbContext.WholesalerProfiles.AsNoTracking()
        join user in dbContext.Users.AsNoTracking() on profile.UserId equals user.Id
        where profile.Address == null || profile.Address.Line1 == null || profile.Address.PostalCode == null
        select new ProfileAddressRow
        {
            UserId = profile.UserId,
            Email = user.Email ?? string.Empty,
            Role = AddressIssueRules.WholesalerRole,
            Name = profile.BusinessName,
            City = profile.Location.City,
            State = profile.Location.State,
            HasAddress = profile.Address != null,
            HasStreet = profile.Address != null && profile.Address.Line1 != null,
            HasPostalCode = profile.Address != null && profile.Address.PostalCode != null,
            CreatedAt = profile.CreatedAt,
        };

    private IQueryable<ProfileAddressRow> Buyers() =>
        from profile in dbContext.BuyerProfiles.AsNoTracking()
        join user in dbContext.Users.AsNoTracking() on profile.UserId equals user.Id
        where profile.Address == null
        select new ProfileAddressRow
        {
            UserId = profile.UserId,
            Email = user.Email ?? string.Empty,
            Role = AddressIssueRules.BuyerRole,
            Name = profile.DisplayName,
            City = profile.City,
            State = profile.State,
            HasAddress = false,
            HasStreet = false,
            HasPostalCode = false,
            CreatedAt = profile.CreatedAt,
        };
}
