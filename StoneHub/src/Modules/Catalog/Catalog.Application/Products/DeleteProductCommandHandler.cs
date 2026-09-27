using Catalog.Domain;
using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed class DeleteProductCommandHandler(IProductRepository productRepository)
    : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, ct);
        if (product is null)
            return Result.NotFound("Product not found.");
        if (product.SellerId != request.CallerSellerId)
            return Result.Forbidden("You do not have permission to modify this listing.");

        product.Delete();
        await productRepository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
