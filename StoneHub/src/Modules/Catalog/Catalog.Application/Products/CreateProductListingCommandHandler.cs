using Catalog.Domain;
using Identity.Application;
using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed class CreateProductListingCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    ISellerDirectory sellerDirectory)
    : IRequestHandler<CreateProductListingCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductListingCommand request, CancellationToken ct)
    {
        var categoryCheck = await ProductCategoryValidation.EnsureValidAsync(
            categoryRepository, request.CategoryId, request.SubcategoryId, ct);
        if (categoryCheck.IsFailure)
            return Result.Failure<Guid>(categoryCheck);

        if (!Enum.TryParse<ListingOwnerType>(request.OwnerType, ignoreCase: true, out var ownerType))
            return Result.Failure<Guid>($"Unknown owner type '{request.OwnerType}'.");
        if (!Enum.TryParse<ProductUnit>(request.Unit, ignoreCase: true, out var unit))
            return Result.Failure<Guid>($"Unknown unit '{request.Unit}'.");

        var price = Money.Create(request.Price, request.Currency);

        var result = Product.CreateListing(
            request.SellerId, ownerType, request.Title, request.CategoryId, request.SubcategoryId,
            request.Size, request.Thickness, request.Finish, request.Color, unit, request.Tags,
            request.QuantityAvailable, price, request.WholesalePrice, request.MinimumOrderQuantity);
        if (result.IsFailure)
            return Result.Failure<Guid>(result);

        var product = result.Value;

        // Denormalize the owner's display name + location for search and cards.
        var seller = await sellerDirectory.GetByUserIdAsync(request.SellerId, ct);
        if (seller is not null)
            product.SetSellerInfo(seller.Name, seller.Location);

        foreach (var url in request.PhotoUrls)
            product.AddMedia(ProductMedia.Create(url, MediaType.Photo));
        foreach (var url in request.VideoUrls)
            product.AddMedia(ProductMedia.Create(url, MediaType.Video));

        productRepository.Add(product);
        await productRepository.SaveChangesAsync(ct);

        return Result.Success(product.Id);
    }
}
