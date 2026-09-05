using Inquiries.Application.Dtos;
using MediatR;

namespace Inquiries.Application;

public sealed record GetSentInquiriesQuery(Guid SellerId) : IRequest<IReadOnlyList<InquiryDto>>;

public sealed class GetSentInquiriesQueryHandler(IInquiryQueryService inquiryQueryService)
    : IRequestHandler<GetSentInquiriesQuery, IReadOnlyList<InquiryDto>>
{
    public Task<IReadOnlyList<InquiryDto>> Handle(GetSentInquiriesQuery request, CancellationToken ct) =>
        inquiryQueryService.GetSentAsync(request.SellerId, ct);
}
