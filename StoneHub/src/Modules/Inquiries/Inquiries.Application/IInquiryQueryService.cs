using Inquiries.Application.Dtos;

namespace Inquiries.Application;

public interface IInquiryQueryService
{
    Task<IReadOnlyList<InquiryDto>> GetReceivedAsync(Guid wholesalerId, CancellationToken ct = default);
    Task<IReadOnlyList<InquiryDto>> GetSentAsync(Guid sellerId, CancellationToken ct = default);
}
