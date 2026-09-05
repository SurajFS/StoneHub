using Catalog.Domain;
using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository)
    : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, ct);
        if (product is null)
            return Result.NotFound("Product not found.");
        if (product.SellerId != request.CallerSellerId)
            return Result.Forbidden("You do not have permission to modify this listing.");

        var categoryCheck = await ProductCategoryValidation.EnsureValidAsync(
            categoryRepository, request.CategoryId, request.SubcategoryId, ct);
        if (categoryCheck.IsFailure)
            return categoryCheck;

        if (!Enum.TryParse<ProductUnit>(request.Unit, ignoreCase: true, out var unit))
            return Result.Failure($"Unknown unit '{request.Unit}'.");

        var price = Money.Create(request.Price, request.Currency);

        var updated = product.UpdateDetails(
            request.Title, request.CategoryId, request.SubcategoryId, request.Size, request.Thickness,
            request.Finish, request.Color, unit, request.Tags, price,
            request.WholesalePrice, request.MinimumOrderQuantity);
        if (updated.IsFailure)
            return updated;

        product.UpdateStock(request.QuantityAvailable);
        await productRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
