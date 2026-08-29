using Catalog.Domain;
using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed class UpdateStockCommandHandler(IProductRepository productRepository)
    : IRequestHandler<UpdateStockCommand, Result>
{
    public async Task<Result> Handle(UpdateStockCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, ct);
        if (product is null)
            return Result.Failure("Product not found.");

        if (product.SellerId != request.CallerSellerId)
            return Result.Failure("You do not have permission to modify this listing.");

        product.UpdateStock(request.QuantityAvailable);
        await productRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
