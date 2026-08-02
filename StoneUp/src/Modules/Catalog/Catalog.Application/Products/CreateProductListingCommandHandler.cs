using Catalog.Domain;
using MediatR;
using SharedKernel;

namespace Catalog.Application.Products;

public sealed class CreateProductListingCommandHandler(IProductRepository productRepository)
    : IRequestHandler<CreateProductListingCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductListingCommand request, CancellationToken ct)
    {
        if (!Enum.TryParse<MaterialType>(request.MaterialType, ignoreCase: true, out var materialType))
            return Result.Failure<Guid>($"Unknown material type '{request.MaterialType}'.");

        var price = Money.Create(request.Price, request.Currency);

        var product = Product.CreateListing(
            request.SellerId,
            request.Title,
            materialType,
            request.Size,
            request.Thickness,
            request.Finish,
            request.QuantityAvailable,
            price);

        foreach (var url in request.PhotoUrls)
            product.AddMedia(ProductMedia.Create(url, MediaType.Photo));

        foreach (var url in request.VideoUrls)
            product.AddMedia(ProductMedia.Create(url, MediaType.Video));

        productRepository.Add(product);
        await productRepository.SaveChangesAsync(ct);

        return Result.Success(product.Id);
    }
}
