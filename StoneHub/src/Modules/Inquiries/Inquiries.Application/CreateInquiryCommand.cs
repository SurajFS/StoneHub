using MediatR;
using SharedKernel;

namespace Inquiries.Application;

public sealed record CreateInquiryCommand(
    Guid SellerId,
    Guid ProductId,
    decimal Quantity,
    string? Message) : IRequest<Result<Guid>>;
