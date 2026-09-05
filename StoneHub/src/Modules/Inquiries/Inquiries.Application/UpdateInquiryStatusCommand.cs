using MediatR;
using SharedKernel;

namespace Inquiries.Application;

public sealed record UpdateInquiryStatusCommand(Guid InquiryId, Guid CallerWholesalerId, string Status) : IRequest<Result>;
