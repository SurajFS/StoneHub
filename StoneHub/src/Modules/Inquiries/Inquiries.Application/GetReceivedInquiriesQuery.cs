using Billing.Application;
using Inquiries.Application.Dtos;
using MediatR;

namespace Inquiries.Application;

public sealed record GetReceivedInquiriesQuery(Guid WholesalerId) : IRequest<IReadOnlyList<InquiryDto>>;

// A Free wholesaler sees that inquiries arrived (product, sender, quantity, date) but not the
// message itself until they subscribe.
public sealed class GetReceivedInquiriesQueryHandler(IInquiryQueryService inquiryQueryService, IPlanEntitlements entitlements)
    : IRequestHandler<GetReceivedInquiriesQuery, IReadOnlyList<InquiryDto>>
{
    public async Task<IReadOnlyList<InquiryDto>> Handle(GetReceivedInquiriesQuery request, CancellationToken ct)
    {
        var inquiries = await inquiryQueryService.GetReceivedAsync(request.WholesalerId, ct);
        if (await entitlements.CanUseOwnerFeaturesAsync(request.WholesalerId, ct))
            return inquiries;

        return inquiries.Select(row => row with { Message = null, IsLocked = true }).ToList();
    }
}
