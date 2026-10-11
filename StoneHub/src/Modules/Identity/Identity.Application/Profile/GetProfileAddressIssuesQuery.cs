using Identity.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Identity.Application.Profile;

// Admin-only: profiles whose address needs correcting (legacy free text, or an owner missing
// street / PIN), so ops can follow up. Read-only; users fix their own address in the app.
public sealed record GetProfileAddressIssuesQuery(string? Role, int Page, int PageSize)
    : IRequest<Result<PagedResult<ProfileAddressIssueDto>>>;
