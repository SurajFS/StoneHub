using Catalog.Domain;
using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed class SetProductActiveCommandHandler(IProductRepository productRepository)
    : IRequestHandler<SetProductActiveCommand, Result>
{
    public async Task<Result> Handle(SetProductActiveCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, ct);
        if (product is null)
            return Result.NotFound("Product not found.");
        if (product.SellerId != request.CallerSellerId)
            return Result.Forbidden("You do not have permission to modify this listing.");

        if (request.IsActive)
            product.Activate();
        else
            product.Deactivate();

        await productRepository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
