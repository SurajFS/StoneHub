using Identity.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Identity.Application.Profile;

public sealed class GetProfileAddressIssuesQueryHandler(IProfileAddressIssueQueryService queryService)
    : IRequestHandler<GetProfileAddressIssuesQuery, Result<PagedResult<ProfileAddressIssueDto>>>
{
    public async Task<Result<PagedResult<ProfileAddressIssueDto>>> Handle(
        GetProfileAddressIssuesQuery request, CancellationToken ct)
    {
        var page = await queryService.SearchAsync(request.Role, request.Page, request.PageSize, ct);

        var rows = page.Items
            .Select(row => new ProfileAddressIssueDto(
                row.UserId, row.Email, row.Role, row.Name, row.City, row.State,
                AddressIssueRules.For(row.Role, row.HasAddress, row.HasStreet, row.HasPostalCode)))
            .ToList();

        return Result.Success(new PagedResult<ProfileAddressIssueDto>(rows, page.Total, page.Page, page.PageSize));
    }
}
