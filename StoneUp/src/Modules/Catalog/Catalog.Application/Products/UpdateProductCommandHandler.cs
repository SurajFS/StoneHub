using Catalog.Domain;
using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed class UpdateProductCommandHandler(IProductRepository productRepository)
    : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, ct);
        if (product is null)
            return Result.Failure("Product not found.");

        if (product.SellerId != request.CallerSellerId)
            return Result.Failure("You do not have permission to modify this listing.");

        if (!Enum.TryParse<MaterialType>(request.MaterialType, ignoreCase: true, out var materialType))
            return Result.Failure($"Unknown material type '{request.MaterialType}'.");

        var price = Money.Create(request.Price, request.Currency);

        product.UpdateDetails(request.Title, materialType, request.Size, request.Thickness, request.Finish, price);
        product.UpdateStock(request.QuantityAvailable);

        await productRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
