using SharedKernel;

namespace Identity.Application.Profile;

public interface IProfileAddressIssueQueryService
{
    // Profiles with at least one AddressIssue (see AddressIssueRules), newest first. `role`
    // (Seller / Wholesaler / Buyer) narrows to one profile type; null means all three.
    Task<PagedResult<ProfileAddressRow>> SearchAsync(string? role, int page, int pageSize, CancellationToken ct = default);
}
