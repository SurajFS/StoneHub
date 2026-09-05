using Catalog.Application;
using Identity.Application;
using Inquiries.Domain;
using MediatR;
using SharedKernel;

namespace Inquiries.Application;

public sealed class CreateInquiryCommandHandler(
    IInquiryRepository inquiryRepository,
    IProductLookup productLookup,
    ISellerDirectory sellerDirectory) : IRequestHandler<CreateInquiryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateInquiryCommand request, CancellationToken ct)
    {
        var product = await productLookup.GetOwnerInfoAsync(request.ProductId, ct);
        if (product is null || !product.IsActive)
            return Result.NotFound<Guid>("Product not found.");
        if (!string.Equals(product.OwnerType, "Wholesaler", StringComparison.OrdinalIgnoreCase))
            return Result.Failure<Guid>("Inquiries can only be sent for wholesale listings.");
        if (product.OwnerId == request.SellerId)
            return Result.Failure<Guid>("You cannot inquire about your own listing.");

        var seller = await sellerDirectory.GetByUserIdAsync(request.SellerId, ct);

        var result = Inquiry.Create(
            request.SellerId, product.OwnerId, request.ProductId, product.Title,
            seller?.Name, request.Quantity, request.Message);
        if (result.IsFailure)
            return Result.Failure<Guid>(result);

        inquiryRepository.Add(result.Value);
        await inquiryRepository.SaveChangesAsync(ct);

        return Result.Success(result.Value.Id);
    }
}
