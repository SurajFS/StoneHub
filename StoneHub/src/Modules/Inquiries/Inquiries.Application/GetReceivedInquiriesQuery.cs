using Inquiries.Application.Dtos;
using MediatR;

namespace Inquiries.Application;

public sealed record GetReceivedInquiriesQuery(Guid WholesalerId) : IRequest<IReadOnlyList<InquiryDto>>;

public sealed class GetReceivedInquiriesQueryHandler(IInquiryQueryService inquiryQueryService)
    : IRequestHandler<GetReceivedInquiriesQuery, IReadOnlyList<InquiryDto>>
{
    public Task<IReadOnlyList<InquiryDto>> Handle(GetReceivedInquiriesQuery request, CancellationToken ct) =>
        inquiryQueryService.GetReceivedAsync(request.WholesalerId, ct);
}
